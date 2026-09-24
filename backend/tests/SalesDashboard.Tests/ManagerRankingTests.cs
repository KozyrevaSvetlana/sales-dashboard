using SalesDashboard.Api.Domain.Analytics;

namespace SalesDashboard.Tests;

public class ManagerRankingTests
{
    private static ManagerStats Manager(int id, decimal revenue, decimal cost, int sales, decimal prevRevenue = 0, decimal prevCost = 0, int prevSales = 0) =>
        new(id, $"Manager {id}", "Team",
            KpiCalculator.Compute(new SalesTotals(revenue, cost, sales)),
            KpiCalculator.Compute(new SalesTotals(prevRevenue, prevCost, prevSales)));

    [Fact]
    public void Sorts_by_gross_profit_descending()
    {
        var ranked = ManagerRanking.Rank(
            [Manager(1, 1_000, 900, 5), Manager(2, 1_000, 500, 5), Manager(3, 1_000, 700, 5)],
            RankingSort.GrossProfit);

        Assert.Equal(new[] { 2, 3, 1 }, ranked.Select(r => r.Stats.ManagerId));
        Assert.Equal(new[] { 1, 2, 3 }, ranked.Select(r => r.Rank));
    }

    [Fact]
    public void Equal_metric_shares_the_same_rank_and_next_rank_is_skipped()
    {
        var ranked = ManagerRanking.Rank(
            [Manager(1, 1_000, 500, 1), Manager(2, 2_000, 1_500, 1), Manager(3, 300, 200, 1)],
            RankingSort.GrossProfit);

        // Прибыль 500, 500, 100 → места 1, 1, 3. При равенстве выше тот, у кого больше выручка.
        Assert.Equal(new[] { 2, 1, 3 }, ranked.Select(r => r.Stats.ManagerId));
        Assert.Equal(new[] { 1, 1, 3 }, ranked.Select(r => r.Rank));
    }

    [Fact]
    public void Manager_without_sales_is_ranked_last_by_average_check()
    {
        var ranked = ManagerRanking.Rank(
            [Manager(1, 0, 0, 0), Manager(2, 900, 500, 3), Manager(3, 1_000, 500, 2)],
            RankingSort.AverageCheck);

        Assert.Equal(new[] { 3, 2, 1 }, ranked.Select(r => r.Stats.ManagerId));
        Assert.Null(ranked[ranked.Count - 1].Stats.Current.AverageCheck);
    }

    [Fact]
    public void Change_is_relative_to_previous_period_of_the_sort_metric()
    {
        var ranked = ManagerRanking.Rank(
            [Manager(1, revenue: 1_200, cost: 600, sales: 1, prevRevenue: 1_000, prevCost: 500, prevSales: 1)],
            RankingSort.GrossProfit);

        Assert.Equal(0.2m, ranked[0].Change); // 600 против 500
    }
}
