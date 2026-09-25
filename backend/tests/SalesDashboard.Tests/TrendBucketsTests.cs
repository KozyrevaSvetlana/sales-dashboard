using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Entities;
using SalesDashboard.Api.Domain.Periods;

namespace SalesDashboard.Tests;

public class TrendBucketsTests
{
    private static DateOnly D(int year, int month, int day) => new(year, month, day);

    [Theory]
    [InlineData(1, TrendGranularity.Day)]
    [InlineData(45, TrendGranularity.Day)]
    [InlineData(46, TrendGranularity.Week)]
    [InlineData(190, TrendGranularity.Week)]
    [InlineData(191, TrendGranularity.Month)]
    public void Auto_granularity_keeps_chart_readable(int days, TrendGranularity expected)
    {
        var range = new DateRange(D(2026, 1, 1), D(2026, 1, 1).AddDays(days));

        Assert.Equal(expected, TrendBuckets.AutoGranularity(range));
    }

    [Fact]
    public void Week_starts_on_monday_like_postgres_date_trunc()
    {
        // 24.09.2026 — четверг, 27.09.2026 — воскресенье
        Assert.Equal(D(2026, 9, 21), TrendBuckets.BucketStart(D(2026, 9, 24), TrendGranularity.Week));
        Assert.Equal(D(2026, 9, 21), TrendBuckets.BucketStart(D(2026, 9, 27), TrendGranularity.Week));
        Assert.Equal(D(2026, 9, 21), TrendBuckets.BucketStart(D(2026, 9, 21), TrendGranularity.Week));
    }

    [Fact]
    public void Month_bucket_is_first_day_of_month()
    {
        Assert.Equal(D(2026, 2, 1), TrendBuckets.BucketStart(D(2026, 2, 28), TrendGranularity.Month));
    }

    [Fact]
    public void Buckets_cover_whole_period_including_partial_first_week()
    {
        // Чт 24.09 — Ср 07.10: недели с 21.09, 28.09, 05.10
        var range = DateRange.Inclusive(D(2026, 9, 24), D(2026, 10, 7));

        var buckets = TrendBuckets.Buckets(range, TrendGranularity.Week);

        Assert.Equal(new[] { D(2026, 9, 21), D(2026, 9, 28), D(2026, 10, 5) }, buckets);
    }

    [Fact]
    public void Fill_adds_zero_points_for_days_without_sales()
    {
        var range = DateRange.Inclusive(D(2026, 9, 1), D(2026, 9, 3));
        var totals = new Dictionary<DateOnly, SalesTotals>
        {
            [D(2026, 9, 2)] = new(Revenue: 1_000m, Cost: 700m, SalesCount: 2),
        };

        var points = TrendBuckets.Fill(range, TrendGranularity.Day, totals);

        Assert.Equal(3, points.Count);
        Assert.Equal(new TrendPoint(D(2026, 9, 1), 0m, 0m, 0), points[0]);
        Assert.Equal(new TrendPoint(D(2026, 9, 2), 1_000m, 300m, 2), points[1]);
        Assert.Equal(new TrendPoint(D(2026, 9, 3), 0m, 0m, 0), points[2]);
    }

    [Fact]
    public void Share_and_margin_are_undefined_for_zero_base()
    {
        Assert.Equal(0.25m, KpiCalculator.Share(25m, 100m));
        Assert.Null(KpiCalculator.Share(0m, 0m));
        Assert.Equal(0.3m, KpiCalculator.Margin(1_000m, 700m));
        Assert.Null(KpiCalculator.Margin(0m, 0m));
    }

    [Theory]
    [InlineData(SaleStatus.Paid, true)]
    [InlineData(SaleStatus.Cancelled, false)]
    [InlineData(SaleStatus.Refunded, false)]
    public void In_memory_status_check_matches_sql_rule(SaleStatus status, bool expected)
    {
        Assert.Equal(expected, SaleRules.CountsTowardsRevenueFor(status));
    }
}
