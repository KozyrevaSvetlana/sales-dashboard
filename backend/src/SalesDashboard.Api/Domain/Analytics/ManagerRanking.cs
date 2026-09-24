namespace SalesDashboard.Api.Domain.Analytics;

public enum RankingSort
{
    GrossProfit,
    AverageCheck,
}

public sealed record ManagerStats(int ManagerId, string FullName, string Team, KpiValues Current, KpiValues Previous);

/// <param name="Rank">Место. При равенстве метрики места совпадают (1, 1, 3).</param>
/// <param name="Change">Относительное изменение метрики сортировки к предыдущему периоду.</param>
public sealed record RankedManager(int Rank, ManagerStats Stats, decimal? Change);

public static class ManagerRanking
{
    public static IReadOnlyList<RankedManager> Rank(IEnumerable<ManagerStats> managers, RankingSort sort)
    {
        var metric = MetricSelector(sort);

        // Вторичные ключи делают порядок детерминированным: при равной метрике
        // строки не «прыгают» между запросами. Менеджеры без значения метрики — в конце.
        var ordered = managers
            .OrderByDescending(m => metric(m.Current) ?? decimal.MinValue)
            .ThenByDescending(m => m.Current.Revenue)
            .ThenBy(m => m.FullName, StringComparer.Ordinal)
            .ThenBy(m => m.ManagerId)
            .ToList();

        var result = new List<RankedManager>(ordered.Count);
        for (var i = 0; i < ordered.Count; i++)
        {
            var manager = ordered[i];
            var sameAsPrevious = i > 0 && metric(ordered[i - 1].Current) == metric(manager.Current);
            var rank = sameAsPrevious ? result[i - 1].Rank : i + 1;

            result.Add(new RankedManager(
                rank,
                manager,
                KpiCalculator.RelativeChange(metric(manager.Current), metric(manager.Previous))));
        }

        return result;
    }

    private static Func<KpiValues, decimal?> MetricSelector(RankingSort sort) => sort switch
    {
        RankingSort.GrossProfit => k => k.GrossProfit,
        RankingSort.AverageCheck => k => k.AverageCheck,
        _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, "Неизвестный режим сортировки"),
    };
}
