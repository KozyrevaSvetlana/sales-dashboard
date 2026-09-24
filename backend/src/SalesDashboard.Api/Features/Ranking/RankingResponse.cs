using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Features.Common;

namespace SalesDashboard.Api.Features.Ranking;

/// <param name="Change">Относительное изменение метрики сортировки к предыдущему периоду.</param>
public sealed record RankingItemDto(
    int Rank,
    int ManagerId,
    string FullName,
    string Team,
    int SalesCount,
    decimal Revenue,
    decimal GrossProfit,
    decimal? Margin,
    decimal? AverageCheck,
    decimal? Change)
{
    public static RankingItemDto Create(RankedManager m) => new(
        m.Rank,
        m.Stats.ManagerId,
        m.Stats.FullName,
        m.Stats.Team,
        m.Stats.Current.SalesCount,
        m.Stats.Current.Revenue,
        m.Stats.Current.GrossProfit,
        m.Stats.Current.Margin,
        m.Stats.Current.AverageCheck,
        m.Change);
}

public sealed record RankingResponse(PeriodDto Period, RankingSort SortBy, IReadOnlyList<RankingItemDto> Items);
