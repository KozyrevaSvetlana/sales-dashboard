using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.Kpi;

/// <param name="Value">Значение за выбранный период (null — не определено, например маржа без выручки).</param>
/// <param name="Previous">Значение за сопоставимый предыдущий период.</param>
/// <param name="Change">
/// Изменение. Для денежных и счётных метрик — относительное (0.12 = +12%),
/// для маржи — абсолютное в долях (0.015 = +1.5 п.п.).
/// </param>
public sealed record MetricDto(decimal? Value, decimal? Previous, decimal? Change);

public sealed record TopManagerDto(int Id, string FullName, decimal GrossProfit);

public sealed record KpiResponse(
    PeriodDto Period,
    MetricDto Revenue,
    MetricDto GrossProfit,
    MetricDto Margin,
    MetricDto SalesCount,
    MetricDto AverageCheck,
    TopManagerDto? TopManager);
