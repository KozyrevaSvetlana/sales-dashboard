using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Periods;
using SalesDashboard.Api.Features.Common;
using SalesDashboard.Api.Infrastructure.Persistence;

namespace SalesDashboard.Api.Features.Trends;

public sealed class TrendsService(AppDbContext db, PeriodResolver periods)
{
    public async Task<TrendsResponse> GetAsync(PeriodQuery query, TrendGranularity? granularity, CancellationToken ct)
    {
        var period = periods.Resolve(query.Preset, query.From, query.To);
        var effectiveGranularity = granularity ?? TrendBuckets.AutoGranularity(period.Current);

        var totalsByBucket = await db.GetTotalsByBucketAsync(
            periods.ToUtc(period.Current), effectiveGranularity, periods.BusinessTimeZoneIanaId, ct);

        var points = TrendBuckets.Fill(period.Current, effectiveGranularity, totalsByBucket);

        return new TrendsResponse(
            PeriodDto.Create(period),
            effectiveGranularity,
            points.Select(TrendPointDto.Create).ToList());
    }
}
