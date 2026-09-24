using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.Ranking;

public static class RankingEndpoints
{
    public static RouteGroupBuilder MapRankingEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/dashboard/managers/ranking",
                ([AsParameters] PeriodQuery query, RankingService service, CancellationToken ct, RankingSort sortBy = RankingSort.GrossProfit) =>
                    service.GetAsync(query, sortBy, ct))
            .WithName("GetManagerRanking")
            .WithTags("Dashboard")
            .WithSummary("Рейтинг менеджеров по валовой прибыли или среднему чеку");

        return api;
    }
}
