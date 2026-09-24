using SalesDashboard.Api.Domain.Entities;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Infrastructure.Persistence;

namespace SalesDashboard.Api.Infrastructure.Seeding;

/// <summary>
/// Генерирует ~3–4 тыс. продаж за 12 месяцев до «сегодня».
///
/// Воспроизводимость: фиксированный seed Random и фиксированный порядок обхода,
/// поэтому после пересоздания БД структура данных та же (даты сдвигаются вместе с «сегодня»,
/// чтобы пресеты «Сегодня» и «7 дней» всегда показывали данные).
/// </summary>
public sealed class DataSeeder(AppDbContext db, PeriodResolver periods, ILogger<DataSeeder> logger)
{
    private const int RandomSeed = 20260924;
    private const int MonthsOfHistory = 12;
    private const int CustomersCount = 80;

    // Сезонность по месяцам (янв…дек): летний пик дронов и предновогодний пик.
    private static readonly double[] MonthSeasonality = [0.75, 0.8, 0.9, 1.0, 1.15, 1.25, 1.2, 1.05, 0.95, 1.0, 1.2, 1.5];
    private const double WeekendFactor = 0.6;

    public async Task SeedAsync(CancellationToken ct)
    {
        var random = new Random(RandomSeed);

        var categories = SeedCatalog.Categories();
        var managers = SeedCatalog.Managers();
        var customers = SeedCatalog.Customers(random, CustomersCount);

        var products = categories.SelectMany(c => c.Products).ToList();
        var bigTicketProducts = products.Where(p => SeedCatalog.BigTicketCategories.Contains(p.Category.Name)).ToList();
        var regularProducts = products.Except(bigTicketProducts).ToList();

        var today = periods.Today();
        var sales = new List<Sale>();

        for (var day = today.AddMonths(-MonthsOfHistory); day <= today; day = day.AddDays(1))
        {
            var dayFactor = MonthSeasonality[day.Month - 1] * (IsWeekend(day) ? WeekendFactor : 1.0);

            foreach (var (manager, profile) in managers)
            {
                var expected = profile.SalesPerDay * dayFactor * profile.ActivityFactor(day, today);
                var salesToday = Poisson(random, expected);

                for (var i = 0; i < salesToday; i++)
                {
                    var customer = customers[random.Next(customers.Count)];
                    sales.Add(CreateSale(random, day, manager, profile, customer, bigTicketProducts, regularProducts));
                }
            }
        }

        db.Categories.AddRange(categories);
        db.Managers.AddRange(managers.Select(m => m.Manager));
        db.Customers.AddRange(customers);
        db.Sales.AddRange(sales);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seed: {Managers} менеджеров, {Customers} клиентов, {Products} товаров, {Sales} продаж",
            managers.Count, customers.Count, products.Count, sales.Count);
    }

    private Sale CreateSale(
        Random random,
        DateOnly day,
        Manager manager,
        ManagerProfile profile,
        Customer customer,
        IReadOnlyList<Product> bigTicketProducts,
        IReadOnlyList<Product> regularProducts)
    {
        var localTime = new TimeOnly(hour: random.Next(9, 21), minute: random.Next(60));
        var soldAtUtc = TimeZoneInfo.ConvertTimeToUtc(
            day.ToDateTime(localTime, DateTimeKind.Unspecified), periods.BusinessTimeZone);

        var sale = new Sale
        {
            Manager = manager,
            Customer = customer,
            SoldAtUtc = soldAtUtc,
            Status = PickStatus(random, profile),
        };

        var positions = random.NextDouble() < 0.6 ? 1 : random.Next(2, 5);
        for (var i = 0; i < positions; i++)
        {
            var isBigTicket = i == 0 && random.NextDouble() < profile.BigTicketChance;
            var pool = isBigTicket ? bigTicketProducts : regularProducts;
            var product = pool[random.Next(pool.Count)];
            var discount = (decimal)(random.NextDouble() * profile.MaxDiscount);

            sale.Items.Add(new SaleItem
            {
                Product = product,
                Quantity = PickQuantity(random, customer.Segment),
                UnitPrice = Math.Round(product.ListPrice * (1 - discount), 2),
                UnitCost = product.UnitCost,
            });
        }

        return sale;
    }

    private static SaleStatus PickStatus(Random random, ManagerProfile profile)
    {
        var roll = random.NextDouble();
        if (roll < profile.CancelRate)
        {
            return SaleStatus.Cancelled;
        }

        return roll < profile.CancelRate + profile.RefundRate ? SaleStatus.Refunded : SaleStatus.Paid;
    }

    private static int PickQuantity(Random random, CustomerSegment segment) => segment switch
    {
        // Изредка — очень крупный корпоративный заказ (граничный случай «очень большая сделка»).
        CustomerSegment.Enterprise => random.NextDouble() < 0.1 ? random.Next(10, 25) : random.Next(1, 6),
        CustomerSegment.Smb => random.Next(1, 4),
        _ => random.NextDouble() < 0.9 ? 1 : 2,
    };

    private static bool IsWeekend(DateOnly day) =>
        day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>Алгоритм Кнута: число событий при среднем lambda (для малых lambda достаточно).</summary>
    private static int Poisson(Random random, double lambda)
    {
        var threshold = Math.Exp(-lambda);
        var count = 0;
        var product = random.NextDouble();
        while (product > threshold)
        {
            count++;
            product *= random.NextDouble();
        }

        return count;
    }
}
