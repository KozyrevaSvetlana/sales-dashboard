using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.Kpi;

public static class KpiEndpoints
{
    public static RouteGroupBuilder MapKpiEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/dashboard/kpi",
                ([AsParameters] PeriodQuery query, KpiService service, CancellationToken ct) =>
                    service.GetAsync(query, ct))
            .WithName("GetKpi")
            .WithTags("Dashboard")
            .WithSummary("KPI за период с динамикой к предыдущему сопоставимому периоду");

        return api;
    }
}
