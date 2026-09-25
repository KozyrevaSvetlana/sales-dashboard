using SalesDashboard.Api.Common;
using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Entities;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Features.Categories;
using SalesDashboard.Api.Features.Common;
using SalesDashboard.Api.Features.Kpi;
using SalesDashboard.Api.Features.Ranking;
using SalesDashboard.Api.Features.RecentSales;
using SalesDashboard.Api.Features.Trends;
using SalesDashboard.Api.Infrastructure.Persistence;
using static SalesDashboard.IntegrationTests.TestData;

namespace SalesDashboard.IntegrationTests;

/// <summary>
/// Проверяем то, что не покрыть юнит-тестами: реальные SQL-запросы к PostgreSQL —
/// фильтр статусов, границы периодов в часовом поясе, группировки, raw SQL графика.
/// «Сегодня» во всех тестах — 24.09.2026, 12:00 по Москве.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class DashboardQueriesTests(PostgresFixture fixture) : IAsyncLifetime
{
    private static readonly DateTimeOffset NowUtc = new(2026, 9, 24, 9, 0, 0, TimeSpan.Zero);

    public Task InitializeAsync() => fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static PeriodResolver Periods() => new(new FixedClock(NowUtc), Moscow);

    private static KpiService Kpi(AppDbContext db) => new(db, Periods(), new RankingService(db, Periods()));

    [Fact]
    public async Task Kpi_counts_only_paid_sales()
    {
        await using (var db = fixture.CreateDbContext())
        {
            var data = new TestData(db);
            var drone = data.Product(data.Category("Дроны"), "Дрон", price: 100m, cost: 60m);
            var anna = data.Manager("Анна");
            var customer = data.Customer();

            data.Sale(anna, customer, Msk(2026, 9, 20), SaleStatus.Paid, (drone, 2, 100m));
            data.Sale(anna, customer, Msk(2026, 9, 20), SaleStatus.Cancelled, (drone, 5, 100m));
            data.Sale(anna, customer, Msk(2026, 9, 21), SaleStatus.Refunded, (drone, 3, 100m));
            await data.SaveAsync();
        }

        await using var queryDb = fixture.CreateDbContext();
        var kpi = await Kpi(queryDb).GetAsync(new PeriodQuery(PeriodPreset.Last7Days), CancellationToken.None);

        Assert.Equal(200m, kpi.Revenue.Value);
        Assert.Equal(80m, kpi.GrossProfit.Value);
        Assert.Equal(1m, kpi.SalesCount.Value);
        Assert.Equal(0.4m, kpi.Margin.Value);
        Assert.Equal(200m, kpi.AverageCheck.Value);
        Assert.Equal("Анна", kpi.TopManager?.FullName);
        Assert.Null(kpi.Revenue.Change); // в прошлом периоде продаж нет — сравнивать не с чем
    }

    [Fact]
    public async Task Period_boundaries_follow_business_time_zone()
    {
        await using (var db = fixture.CreateDbContext())
        {
            var data = new TestData(db);
            var drone = data.Product(data.Category("Дроны"), "Дрон", 100m, 60m);
            var anna = data.Manager("Анна");
            var customer = data.Customer();

            data.Sale(anna, customer, Msk(2026, 9, 23, 23, 59), SaleStatus.Paid, (drone, 1, 100m)); // вчера
            data.Sale(anna, customer, Msk(2026, 9, 24, 0, 0), SaleStatus.Paid, (drone, 1, 100m));   // сегодня, первая минута
            data.Sale(anna, customer, Msk(2026, 9, 24, 23, 59), SaleStatus.Paid, (drone, 1, 100m)); // сегодня, последняя минута
            data.Sale(anna, customer, Msk(2026, 9, 25, 0, 0), SaleStatus.Paid, (drone, 1, 100m));   // завтра
            await data.SaveAsync();
        }

        await using var queryDb = fixture.CreateDbContext();
        var kpi = await Kpi(queryDb).GetAsync(new PeriodQuery(PeriodPreset.Today), CancellationToken.None);

        Assert.Equal(2m, kpi.SalesCount.Value);
        Assert.Equal(1m, kpi.SalesCount.Previous);
    }

    [Fact]
    public async Task Ranking_keeps_active_managers_without_sales_and_hides_idle_inactive_ones()
    {
        await using (var db = fixture.CreateDbContext())
        {
            var data = new TestData(db);
            var drone = data.Product(data.Category("Дроны"), "Дрон", 100m, 60m);
            var customer = data.Customer();
            var anna = data.Manager("Анна");
            data.Manager("Борис");                                 // активен, продаж нет
            data.Manager("Виктор", isActive: false);               // уволен, продаж нет
            var galina = data.Manager("Галина", isActive: false);  // уволена, но продавала в периоде

            data.Sale(anna, customer, Msk(2026, 9, 20), SaleStatus.Paid, (drone, 1, 100m));
            data.Sale(anna, customer, Msk(2026, 9, 21), SaleStatus.Paid, (drone, 1, 100m));
            data.Sale(galina, customer, Msk(2026, 9, 22), SaleStatus.Paid, (drone, 1, 100m));
            await data.SaveAsync();
        }

        await using var queryDb = fixture.CreateDbContext();
        var ranking = await new RankingService(queryDb, Periods())
            .GetAsync(new PeriodQuery(PeriodPreset.Last30Days), RankingSort.GrossProfit, CancellationToken.None);

        Assert.Equal(new[] { "Анна", "Галина", "Борис" }, ranking.Items.Select(i => i.FullName));
        Assert.Equal(2, ranking.Items[0].SalesCount);

        var boris = ranking.Items[2];
        Assert.Equal(0, boris.SalesCount);
        Assert.Equal(0m, boris.GrossProfit);
        Assert.Null(boris.AverageCheck);
    }

    [Fact]
    public async Task Trend_points_are_zero_filled_and_sum_up_to_kpi()
    {
        await using (var db = fixture.CreateDbContext())
        {
            var data = new TestData(db);
            var drone = data.Product(data.Category("Дроны"), "Дрон", 100m, 60m);
            var anna = data.Manager("Анна");
            var customer = data.Customer();

            data.Sale(anna, customer, Msk(2026, 9, 20), SaleStatus.Paid, (drone, 2, 100m));
            // 01:00 по Москве 22.09 = 22:00 UTC 21.09 — должна попасть в точку 22.09
            data.Sale(anna, customer, Msk(2026, 9, 22, 1, 0), SaleStatus.Paid, (drone, 1, 100m));
            data.Sale(anna, customer, Msk(2026, 9, 22, 15, 0), SaleStatus.Cancelled, (drone, 9, 100m));
            await data.SaveAsync();
        }

        await using var queryDb = fixture.CreateDbContext();
        var query = new PeriodQuery(PeriodPreset.Last7Days);
        var trends = await new TrendsService(queryDb, Periods()).GetAsync(query, granularity: null, CancellationToken.None);
        var kpi = await Kpi(queryDb).GetAsync(query, CancellationToken.None);

        Assert.Equal(TrendGranularity.Day, trends.Granularity);
        Assert.Equal(7, trends.Points.Count);
        Assert.Equal(kpi.Revenue.Value, trends.Points.Sum(p => p.Revenue));
        Assert.Equal(kpi.SalesCount.Value, trends.Points.Sum(p => p.SalesCount));

        Assert.Equal(0m, trends.Points.Single(p => p.Date == new DateOnly(2026, 9, 21)).Revenue);
        var sep22 = trends.Points.Single(p => p.Date == new DateOnly(2026, 9, 22));
        Assert.Equal(100m, sep22.Revenue);
        Assert.Equal(1, sep22.SalesCount);
    }

    [Fact]
    public async Task Weekly_trend_matches_kpi_for_custom_period()
    {
        await using (var db = fixture.CreateDbContext())
        {
            var data = new TestData(db);
            var drone = data.Product(data.Category("Дроны"), "Дрон", 100m, 60m);
            var anna = data.Manager("Анна");
            var customer = data.Customer();

            data.Sale(anna, customer, Msk(2026, 9, 1), SaleStatus.Paid, (drone, 1, 100m));
            data.Sale(anna, customer, Msk(2026, 9, 15), SaleStatus.Paid, (drone, 2, 100m));
            data.Sale(anna, customer, Msk(2026, 9, 30, 23, 30), SaleStatus.Paid, (drone, 3, 100m));
            await data.SaveAsync();
        }

        await using var queryDb = fixture.CreateDbContext();
        var query = new PeriodQuery(PeriodPreset.Custom, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));
        var trends = await new TrendsService(queryDb, Periods()).GetAsync(query, TrendGranularity.Week, CancellationToken.None);
        var kpi = await Kpi(queryDb).GetAsync(query, CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 8, 31), trends.Points[0].Date); // понедельник недели, в которую входит 01.09
        Assert.Equal(600m, kpi.Revenue.Value);
        Assert.Equal(kpi.Revenue.Value, trends.Points.Sum(p => p.Revenue));
    }

    [Fact]
    public async Task Categories_have_revenue_share_and_top_products()
    {
        await using (var db = fixture.CreateDbContext())
        {
            var data = new TestData(db);
            var drones = data.Category("Дроны");
            var accessories = data.Category("Аксессуары");
            data.Category("Сервис"); // без продаж — всё равно должна быть в списке

            var bigDrone = data.Product(drones, "Большой дрон", 90m, 60m);
            var smallDrone = data.Product(drones, "Малый дрон", 120m, 20m);
            var filter = data.Product(accessories, "Фильтр", 10m, 5m);
            var anna = data.Manager("Анна");
            var customer = data.Customer();

            data.Sale(anna, customer, Msk(2026, 9, 20), SaleStatus.Paid, (bigDrone, 2, 90m), (smallDrone, 1, 120m));
            data.Sale(anna, customer, Msk(2026, 9, 21), SaleStatus.Paid, (filter, 10, 10m));
            data.Sale(anna, customer, Msk(2026, 9, 22), SaleStatus.Cancelled, (filter, 100, 10m));
            await data.SaveAsync();
        }

        await using var queryDb = fixture.CreateDbContext();
        var result = await new CategoriesService(queryDb, Periods())
            .GetAsync(new PeriodQuery(PeriodPreset.Last7Days), top: 2, CancellationToken.None);

        Assert.Equal(new[] { "Дроны", "Аксессуары", "Сервис" }, result.Categories.Select(c => c.Name));

        var dronesDto = result.Categories[0];
        Assert.Equal(300m, dronesDto.Revenue);
        Assert.Equal(160m, dronesDto.GrossProfit); // 300 − (2×60 + 1×20)
        Assert.Equal(0.75m, dronesDto.Share);
        Assert.Equal(3, dronesDto.Units);

        Assert.Equal(0.25m, result.Categories[1].Share);
        Assert.Equal(0m, result.Categories[2].Revenue);
        Assert.Null(result.Categories[2].Margin);

        Assert.Equal(new[] { "Большой дрон", "Малый дрон" }, result.TopProducts.Select(p => p.Name));
        Assert.Equal(180m, result.TopProducts[0].Revenue);
    }

    [Fact]
    public async Task Recent_sales_are_newest_first_and_include_all_statuses()
    {
        await using (var db = fixture.CreateDbContext())
        {
            var data = new TestData(db);
            var category = data.Category("Дроны");
            var drone = data.Product(category, "Дрон", 100m, 60m);
            var bag = data.Product(category, "Кейс", 50m, 20m);
            var anna = data.Manager("Анна");
            var customer = data.Customer("Иван Петров");

            data.Sale(anna, customer, Msk(2026, 9, 20, 10, 0), SaleStatus.Paid, (drone, 1, 100m));
            data.Sale(anna, customer, Msk(2026, 9, 21, 10, 0), SaleStatus.Cancelled, (drone, 2, 100m));
            data.Sale(anna, customer, Msk(2026, 9, 22, 10, 0), SaleStatus.Refunded, (bag, 1, 50m), (drone, 1, 100m));
            await data.SaveAsync();
        }

        await using var queryDb = fixture.CreateDbContext();
        var result = await new RecentSalesService(queryDb, Periods())
            .GetAsync(new PeriodQuery(PeriodPreset.Last7Days), limit: 2, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);

        var refunded = result.Items[0];
        Assert.Equal(SaleStatus.Refunded, refunded.Status);
        Assert.False(refunded.CountsTowardsRevenue);
        Assert.Equal(new[] { "Дрон", "Кейс" }, refunded.Products); // дорогая позиция первой
        Assert.Equal(TimeSpan.FromHours(3), refunded.SoldAt.Offset);
        Assert.Equal(10, refunded.SoldAt.Hour); // время по Москве, а не UTC

        var cancelled = result.Items[1];
        Assert.Equal(SaleStatus.Cancelled, cancelled.Status);
        Assert.Equal(200m, cancelled.Amount);
        Assert.Equal(80m, cancelled.GrossProfit);
        Assert.Equal(2, cancelled.Units);
        Assert.Equal("Иван Петров", cancelled.CustomerName);
    }

    [Fact]
    public async Task Recent_sales_limit_is_validated()
    {
        await using var queryDb = fixture.CreateDbContext();
        var service = new RecentSalesService(queryDb, Periods());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.GetAsync(new PeriodQuery(PeriodPreset.Today), limit: 0, CancellationToken.None));
    }
}
