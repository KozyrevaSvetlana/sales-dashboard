using SalesDashboard.Api.Domain.Entities;
using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.RecentSales;

/// <param name="SoldAt">Время продажи в часовом поясе компании (с указанием смещения).</param>
/// <param name="CountsTowardsRevenue">Учитывается ли продажа в выручке (по SaleRules). Отменённые и возвраты — false.</param>
/// <param name="Products">Названия товаров, от самой дорогой позиции к самой дешёвой.</param>
/// <param name="Amount">Сумма заказа (по ценам продажи).</param>
/// <param name="GrossProfit">Прибыль заказа: сумма − себестоимость.</param>
public sealed record RecentSaleDto(
    int Id,
    DateTimeOffset SoldAt,
    SaleStatus Status,
    bool CountsTowardsRevenue,
    string ManagerName,
    string CustomerName,
    string CustomerCompany,
    IReadOnlyList<string> Products,
    int Units,
    decimal Amount,
    decimal GrossProfit);

public sealed record RecentSalesResponse(PeriodDto Period, IReadOnlyList<RecentSaleDto> Items);
