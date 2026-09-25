using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.Trends;

/// <param name="Date">Начало интервала: день, понедельник недели или 1-е число месяца.</param>
public sealed record TrendPointDto(DateOnly Date, decimal Revenue, decimal GrossProfit, int SalesCount)
{
    public static TrendPointDto Create(TrendPoint point) =>
        new(point.Bucket, point.Revenue, point.GrossProfit, point.SalesCount);
}

public sealed record TrendsResponse(PeriodDto Period, TrendGranularity Granularity, IReadOnlyList<TrendPointDto> Points);
