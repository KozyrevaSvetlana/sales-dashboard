using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Features.Common;
using SalesDashboard.Api.Infrastructure.Persistence;

namespace SalesDashboard.Api.Features.Ranking;

public sealed class RankingService(AppDbContext db, PeriodResolver periods)
{
    public async Task<RankingResponse> GetAsync(PeriodQuery query, RankingSort sortBy, CancellationToken ct)
    {
        var period = periods.Resolve(query.Preset, query.From, query.To);
        var ranked = await RankAsync(period, sortBy, ct);

        return new RankingResponse(PeriodDto.Create(period), sortBy, ranked.Select(RankingItemDto.Create).ToList());
    }

    public async Task<IReadOnlyList<RankedManager>> RankAsync(ReportingPeriod period, RankingSort sortBy, CancellationToken ct)
    {
        var current = await db.Sales.AsNoTracking()
            .CountedInRange(periods.ToUtc(period.Current))
            .GetTotalsByManagerAsync(ct);

        var previous = await db.Sales.AsNoTracking()
            .CountedInRange(periods.ToUtc(period.Previous))
            .GetTotalsByManagerAsync(ct);

        // Менеджеров ~20, поэтому «LEFT JOIN» делаем в памяти: так в рейтинг попадают
        // и менеджеры без продаж за период (с нулями), а не пропадают из таблицы.
        var managers = await db.Managers.AsNoTracking()
            .OrderBy(m => m.Id)
            .Select(m => new { m.Id, m.FullName, m.Team, m.IsActive })
            .ToListAsync(ct);

        var stats = managers
            .Where(m => m.IsActive || current.ContainsKey(m.Id)) // уволенные — только если есть продажи в периоде
            .Select(m => new ManagerStats(
                m.Id,
                m.FullName,
                m.Team,
                KpiCalculator.Compute(current.GetValueOrDefault(m.Id, SalesTotals.Empty)),
                KpiCalculator.Compute(previous.GetValueOrDefault(m.Id, SalesTotals.Empty))));

        return ManagerRanking.Rank(stats, sortBy);
    }
}
