using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Entities;
using SalesDashboard.Api.Domain.Periods;

namespace SalesDashboard.Api.Infrastructure.Persistence;

/// <summary>
/// Переиспользуемые куски запросов. Всё остаётся IQueryable до последнего шага,
/// так что фильтрация и агрегация выполняются в PostgreSQL, а не в памяти.
/// </summary>
public static class SalesQueries
{
    /// <summary>Все продажи (любой статус) внутри периода [from, to).</summary>
    public static IQueryable<Sale> InRange(this IQueryable<Sale> sales, UtcRange range) =>
        sales.Where(s => s.SoldAtUtc >= range.FromUtc && s.SoldAtUtc < range.ToUtcExclusive);

    /// <summary>Продажи, которые учитываются в выручке, внутри периода [from, to).</summary>
    public static IQueryable<Sale> CountedInRange(this IQueryable<Sale> sales, UtcRange range) =>
        sales
            .Where(SaleRules.CountsTowardsRevenue)
            .InRange(range);

    /// <summary>Три компактных агрегатных запроса вместо загрузки продаж в память.</summary>
    public static async Task<SalesTotals> GetTotalsAsync(this IQueryable<Sale> sales, CancellationToken ct)
    {
        var salesCount = await sales.CountAsync(ct);
        if (salesCount == 0)
        {
            return SalesTotals.Empty;
        }

        var items = sales.SelectMany(s => s.Items);
        var revenue = await items.SumAsync(i => i.Quantity * i.UnitPrice, ct);
        var cost = await items.SumAsync(i => i.Quantity * i.UnitCost, ct);

        return new SalesTotals(revenue, cost, salesCount);
    }

    /// <summary>Агрегаты по каждому менеджеру за период: две группировки на стороне БД.</summary>
    public static async Task<Dictionary<int, SalesTotals>> GetTotalsByManagerAsync(
        this IQueryable<Sale> sales, CancellationToken ct)
    {
        var counts = await sales
            .GroupBy(s => s.ManagerId)
            .Select(g => new { ManagerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ManagerId, x => x.Count, ct);

        var sums = await sales
            .SelectMany(s => s.Items)
            .GroupBy(i => i.Sale.ManagerId)
            .Select(g => new
            {
                ManagerId = g.Key,
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
                Cost = g.Sum(i => i.Quantity * i.UnitCost),
            })
            .ToDictionaryAsync(x => x.ManagerId, ct);

        return counts.ToDictionary(
            c => c.Key,
            c => sums.TryGetValue(c.Key, out var s)
                ? new SalesTotals(s.Revenue, s.Cost, c.Value)
                : new SalesTotals(0m, 0m, c.Value));
    }
}
