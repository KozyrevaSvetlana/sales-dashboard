using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Common;
using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Features.Common;
using SalesDashboard.Api.Infrastructure.Persistence;

namespace SalesDashboard.Api.Features.RecentSales;

/// <summary>
/// Последние продажи периода — все статусы, чтобы руководитель видел и отмены с возвратами.
/// Одна проекция в DTO: EF строит один SQL-запрос с подзапросами, без N+1.
/// </summary>
public sealed class RecentSalesService(AppDbContext db, PeriodResolver periods)
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 100;

    public async Task<RecentSalesResponse> GetAsync(PeriodQuery query, int limit, CancellationToken ct)
    {
        if (limit is < 1 or > MaxLimit)
        {
            throw new BadRequestException($"Параметр limit должен быть от 1 до {MaxLimit}.");
        }

        var period = periods.Resolve(query.Preset, query.From, query.To);

        var rows = await db.Sales.AsNoTracking()
            .InRange(periods.ToUtc(period.Current))
            .OrderByDescending(s => s.SoldAtUtc)
            .ThenByDescending(s => s.Id)
            .Take(limit)
            .Select(s => new
            {
                s.Id,
                s.SoldAtUtc,
                s.Status,
                ManagerName = s.Manager.FullName,
                CustomerName = s.Customer.Name,
                CustomerCompany = s.Customer.Company,
                Products = s.Items
                    .OrderByDescending(i => i.Quantity * i.UnitPrice)
                    .Select(i => i.Product.Name)
                    .ToList(),
                Units = s.Items.Sum(i => i.Quantity),
                Amount = s.Items.Sum(i => i.Quantity * i.UnitPrice),
                Cost = s.Items.Sum(i => i.Quantity * i.UnitCost),
            })
            .ToListAsync(ct);

        var items = rows
            .Select(r => new RecentSaleDto(
                r.Id,
                ToBusinessTime(r.SoldAtUtc),
                r.Status,
                SaleRules.CountsTowardsRevenueFor(r.Status),
                r.ManagerName,
                r.CustomerName,
                r.CustomerCompany,
                r.Products,
                r.Units,
                r.Amount,
                r.Amount - r.Cost))
            .ToList();

        return new RecentSalesResponse(PeriodDto.Create(period), items);
    }

    private DateTimeOffset ToBusinessTime(DateTime soldAtUtc)
    {
        var utc = DateTime.SpecifyKind(soldAtUtc, DateTimeKind.Utc);
        var timeZone = periods.BusinessTimeZone;
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone), timeZone.GetUtcOffset(utc));
    }
}
