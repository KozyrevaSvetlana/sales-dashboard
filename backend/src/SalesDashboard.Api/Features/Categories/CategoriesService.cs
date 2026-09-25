using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Common;
using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Features.Common;
using SalesDashboard.Api.Infrastructure.Persistence;

namespace SalesDashboard.Api.Features.Categories;

public sealed class CategoriesService(AppDbContext db, PeriodResolver periods)
{
    public const int DefaultTop = 5;
    public const int MaxTop = 50;

    public async Task<CategoriesResponse> GetAsync(PeriodQuery query, int top, CancellationToken ct)
    {
        if (top is < 1 or > MaxTop)
        {
            throw new BadRequestException($"Параметр top должен быть от 1 до {MaxTop}.");
        }

        var period = periods.Resolve(query.Preset, query.From, query.To);
        var items = db.Sales.AsNoTracking()
            .CountedInRange(periods.ToUtc(period.Current))
            .SelectMany(s => s.Items);

        // Агрегация по категориям — в БД одной группировкой.
        var totalsByCategory = await items
            .GroupBy(i => i.Product.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
                Cost = g.Sum(i => i.Quantity * i.UnitCost),
                Units = g.Sum(i => i.Quantity),
            })
            .ToDictionaryAsync(x => x.CategoryId, ct);

        // Категорий единицы, поэтому «LEFT JOIN» в памяти: категории без продаж тоже видны (с нулями).
        var categories = await db.Categories.AsNoTracking()
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);

        var totalRevenue = totalsByCategory.Values.Sum(x => x.Revenue);

        var categoryDtos = categories
            .Select(c =>
            {
                totalsByCategory.TryGetValue(c.Id, out var t);
                var revenue = t?.Revenue ?? 0m;
                var cost = t?.Cost ?? 0m;
                return new CategorySalesDto(
                    c.Id,
                    c.Name,
                    revenue,
                    revenue - cost,
                    KpiCalculator.Margin(revenue, cost),
                    KpiCalculator.Share(revenue, totalRevenue),
                    t?.Units ?? 0);
            })
            .OrderByDescending(c => c.Revenue)
            .ThenBy(c => c.Name, StringComparer.Ordinal)
            .ToList();

        // Топ товаров по выручке — группировка, сортировка и ограничение тоже в БД.
        var topProducts = await items
            .GroupBy(i => new { i.ProductId, i.Product.Name, CategoryName = i.Product.Category.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Name,
                g.Key.CategoryName,
                Units = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
                Cost = g.Sum(i => i.Quantity * i.UnitCost),
            })
            .OrderByDescending(x => x.Revenue)
            .ThenBy(x => x.ProductId)
            .Take(top)
            .ToListAsync(ct);

        var productDtos = topProducts
            .Select(p => new ProductSalesDto(
                p.ProductId,
                p.Name,
                p.CategoryName,
                p.Units,
                p.Revenue,
                p.Revenue - p.Cost,
                KpiCalculator.Margin(p.Revenue, p.Cost)))
            .ToList();

        return new CategoriesResponse(PeriodDto.Create(period), categoryDtos, productDtos);
    }
}
