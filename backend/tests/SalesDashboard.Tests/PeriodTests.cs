using SalesDashboard.Api.Common;
using SalesDashboard.Api.Domain.Periods;

namespace SalesDashboard.Tests;

public class PeriodTests
{
    private static readonly TimeZoneInfo Moscow = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

    private static PeriodResolver ResolverAt(FixedClock clock) => new(clock, Moscow);

    private static DateOnly D(int year, int month, int day) => new(year, month, day);

    [Fact]
    public void Today_uses_business_time_zone_not_utc()
    {
        // 22:30 UTC 23 сентября = 01:30 МСК 24 сентября
        var resolver = ResolverAt(FixedClock.AtUtc(2026, 9, 23, hour: 22, minute: 30));

        var period = resolver.Resolve(PeriodPreset.Today);

        Assert.Equal(D(2026, 9, 24), period.Current.From);
        Assert.Equal(D(2026, 9, 23), period.Previous.From);
    }

    [Fact]
    public void Last7Days_includes_today_and_previous_period_is_adjacent()
    {
        var period = ResolverAt(FixedClock.AtUtc(2026, 9, 24)).Resolve(PeriodPreset.Last7Days);

        Assert.Equal(D(2026, 9, 18), period.Current.From);
        Assert.Equal(D(2026, 9, 24), period.Current.ToInclusive);
        Assert.Equal(7, period.Previous.Days);
        Assert.Equal(period.Current.From, period.Previous.ToExclusive); // без пересечения и без дыры
    }

    [Fact]
    public void CurrentMonth_is_compared_with_same_days_of_previous_month()
    {
        var period = ResolverAt(FixedClock.AtUtc(2026, 9, 24)).Resolve(PeriodPreset.CurrentMonth);

        Assert.Equal(DateRange.Inclusive(D(2026, 9, 1), D(2026, 9, 24)), period.Current);
        Assert.Equal(DateRange.Inclusive(D(2026, 8, 1), D(2026, 8, 24)), period.Previous);
    }

    [Fact]
    public void CurrentMonth_comparison_does_not_overflow_shorter_previous_month()
    {
        var period = ResolverAt(FixedClock.AtUtc(2026, 3, 31)).Resolve(PeriodPreset.CurrentMonth);

        Assert.Equal(DateRange.Inclusive(D(2026, 2, 1), D(2026, 2, 28)), period.Previous);
    }

    [Fact]
    public void PreviousMonth_is_full_calendar_month()
    {
        var period = ResolverAt(FixedClock.AtUtc(2026, 1, 15)).Resolve(PeriodPreset.PreviousMonth);

        Assert.Equal(DateRange.Inclusive(D(2025, 12, 1), D(2025, 12, 31)), period.Current);
        Assert.Equal(DateRange.Inclusive(D(2025, 11, 1), D(2025, 11, 30)), period.Previous);
    }

    [Fact]
    public void Custom_range_is_inclusive_of_both_dates()
    {
        var period = ResolverAt(FixedClock.AtUtc(2026, 9, 24))
            .Resolve(PeriodPreset.Custom, D(2026, 9, 10), D(2026, 9, 10));

        Assert.Equal(1, period.Current.Days);
    }

    [Theory]
    [InlineData(null, "2026-09-10")]
    [InlineData("2026-09-10", null)]
    [InlineData("2026-09-10", "2026-09-09")]
    [InlineData("2025-01-01", "2026-09-01")]
    public void Custom_range_rejects_invalid_input(string? from, string? to)
    {
        var resolver = ResolverAt(FixedClock.AtUtc(2026, 9, 24));

        Assert.Throws<BadRequestException>(() => resolver.Resolve(
            PeriodPreset.Custom,
            from is null ? null : DateOnly.ParseExact(from, "yyyy-MM-dd"),
            to is null ? null : DateOnly.ParseExact(to, "yyyy-MM-dd")));
    }

    [Fact]
    public void Utc_bounds_start_at_business_midnight_and_are_half_open()
    {
        var range = DateRange.Inclusive(D(2026, 9, 24), D(2026, 9, 24));

        var utc = range.ToUtc(Moscow);

        // Полночь МСК = 21:00 UTC предыдущего дня
        Assert.Equal(new DateTime(2026, 9, 23, 21, 0, 0, DateTimeKind.Utc), utc.FromUtc);
        Assert.Equal(new DateTime(2026, 9, 24, 21, 0, 0, DateTimeKind.Utc), utc.ToUtcExclusive);
    }
}
