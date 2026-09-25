using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.RecentSales;

public static class RecentSalesEndpoints
{
    public static RouteGroupBuilder MapRecentSalesEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/dashboard/sales/recent",
                ([AsParameters] PeriodQuery query, RecentSalesService service, CancellationToken ct, int limit = RecentSalesService.DefaultLimit) =>
                    service.GetAsync(query, limit, ct))
            .WithName("GetRecentSales")
            .WithTags("Dashboard")
            .WithSummary("Последние продажи периода (все статусы), новые сверху");

        return api;
    }
}
