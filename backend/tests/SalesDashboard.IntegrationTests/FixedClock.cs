namespace SalesDashboard.IntegrationTests;

internal sealed class FixedClock(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
