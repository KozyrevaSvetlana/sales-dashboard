namespace SalesDashboard.Tests;

/// <summary>Часы, которые всегда показывают заданное время. Своя реализация вместо FakeTimeProvider — без лишнего пакета.</summary>
internal sealed class FixedClock(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;

    public static FixedClock AtUtc(int year, int month, int day, int hour = 12, int minute = 0) =>
        new(new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero));
}
