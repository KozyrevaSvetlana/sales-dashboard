namespace SalesDashboard.Api.Domain.Analytics;

/// <summary>Сырые агрегаты, которые отдаёт БД. Всё производное считается в KpiCalculator.</summary>
public sealed record SalesTotals(decimal Revenue, decimal Cost, int SalesCount)
{
    public static readonly SalesTotals Empty = new(0m, 0m, 0);
}

/// <summary>
/// Метрики за период. Margin — доля (0.25 = 25%).
/// null означает «не определено» (например, маржа при нулевой выручке), а не ноль.
/// </summary>
public sealed record KpiValues(
    decimal Revenue,
    decimal GrossProfit,
    decimal? Margin,
    int SalesCount,
    decimal? AverageCheck);

/// <summary>
/// Чистые функции без БД и без состояния — поэтому покрыты юнит-тестами целиком.
/// </summary>
public static class KpiCalculator
{
    public static KpiValues Compute(SalesTotals totals)
    {
        var grossProfit = totals.Revenue - totals.Cost;

        return new KpiValues(
            Revenue: totals.Revenue,
            GrossProfit: grossProfit,
            Margin: totals.Revenue == 0m ? null : grossProfit / totals.Revenue,
            SalesCount: totals.SalesCount,
            AverageCheck: totals.SalesCount == 0 ? null : totals.Revenue / totals.SalesCount);
    }

    /// <summary>
    /// Относительное изменение (0.1 = +10%). null, если база нулевая или значения не определены:
    /// «+∞%» пользователю ничего не говорит.
    /// </summary>
    public static decimal? RelativeChange(decimal? current, decimal? previous)
    {
        if (current is null || previous is null || previous.Value == 0m)
        {
            return null;
        }

        return (current.Value - previous.Value) / Math.Abs(previous.Value);
    }

    /// <summary>
    /// Абсолютная разница — для метрик, которые сами являются долями (маржа):
    /// рост маржи с 20% до 25% — это +5 п.п., а не +25%.
    /// </summary>
    public static decimal? AbsoluteChange(decimal? current, decimal? previous) =>
        current is null || previous is null ? null : current.Value - previous.Value;
}
