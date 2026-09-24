using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Features.Common;
using SalesDashboard.Api.Features.Ranking;
using SalesDashboard.Api.Infrastructure.Persistence;

namespace SalesDashboard.Api.Features.Kpi;

public sealed class KpiService(AppDbContext db, PeriodResolver periods, RankingService ranking)
{
    public async Task<KpiResponse> GetAsync(PeriodQuery query, CancellationToken ct)
    {
        var period = periods.Resolve(query.Preset, query.From, query.To);

        // Запросы к одному DbContext выполняются последовательно — он не потокобезопасен.
        var current = KpiCalculator.Compute(await LoadTotalsAsync(period.Current, ct));
        var previous = KpiCalculator.Compute(await LoadTotalsAsync(period.Previous, ct));
        var topManager = await FindTopManagerAsync(period, ct);

        return new KpiResponse(
            PeriodDto.Create(period),
            Revenue: Relative(current.Revenue, previous.Revenue),
            GrossProfit: Relative(current.GrossProfit, previous.GrossProfit),
            Margin: new MetricDto(current.Margin, previous.Margin, KpiCalculator.AbsoluteChange(current.Margin, previous.Margin)),
            SalesCount: Relative(current.SalesCount, previous.SalesCount),
            AverageCheck: Relative(current.AverageCheck, previous.AverageCheck),
            TopManager: topManager);
    }

    private Task<SalesTotals> LoadTotalsAsync(DateRange range, CancellationToken ct) =>
        db.Sales.AsNoTracking().CountedInRange(periods.ToUtc(range)).GetTotalsAsync(ct);

    private async Task<TopManagerDto?> FindTopManagerAsync(ReportingPeriod period, CancellationToken ct)
    {
        // Переиспользуем рейтинг, а не дублируем логику «кто лучший» — одно правило, один источник.
        var leader = (await ranking.RankAsync(period, RankingSort.GrossProfit, ct))
            .FirstOrDefault(m => m.Stats.Current.SalesCount > 0);

        return leader is null
            ? null
            : new TopManagerDto(leader.Stats.ManagerId, leader.Stats.FullName, leader.Stats.Current.GrossProfit);
    }

    private static MetricDto Relative(decimal? current, decimal? previous) =>
        new(current, previous, KpiCalculator.RelativeChange(current, previous));
}
