using System.Linq.Expressions;
using SalesDashboard.Api.Domain.Entities;

namespace SalesDashboard.Api.Domain.Analytics;

/// <summary>
/// Единственное место, где определено, какие продажи попадают в выручку.
///
/// Трактовка статусов (продублирована в README):
///  • Paid      — учитывается во всех метриках;
///  • Cancelled — продажа не состоялась, не учитывается нигде;
///  • Refunded  — полный возврат: выручка и прибыль по ней = 0, в количество продаж не входит.
///
/// Это Expression, а не метод: EF Core транслирует его в SQL WHERE, фильтрация идёт в БД.
/// </summary>
public static class SaleRules
{
    public static readonly Expression<Func<Sale, bool>> CountsTowardsRevenue =
        sale => sale.Status == SaleStatus.Paid;
}
