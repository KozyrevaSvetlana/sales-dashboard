using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.Trends;

public static class TrendsEndpoints
{
    public static RouteGroupBuilder MapTrendsEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/dashboard/trends",
                ([AsParameters] PeriodQuery query, TrendsService service, CancellationToken ct, TrendGranularity? granularity = null) =>
                    service.GetAsync(query, granularity, ct))
            .WithName("GetTrends")
            .WithTags("Dashboard")
            .WithSummary("Выручка, прибыль и количество продаж по дням/неделям/месяцам (без параметра granularity — автоматически)");

        return api;
    }
}
