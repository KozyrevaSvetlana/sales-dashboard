using SalesDashboard.Api.Domain.Analytics;
using SalesDashboard.Api.Domain.Entities;

namespace SalesDashboard.Tests;

public class KpiCalculatorTests
{
    [Fact]
    public void Computes_gross_profit_margin_and_average_check()
    {
        var kpi = KpiCalculator.Compute(new SalesTotals(Revenue: 1_000m, Cost: 750m, SalesCount: 4));

        Assert.Equal(250m, kpi.GrossProfit);
        Assert.Equal(0.25m, kpi.Margin);
        Assert.Equal(250m, kpi.AverageCheck);
    }

    [Fact]
    public void Empty_period_has_zero_money_and_undefined_ratios()
    {
        var kpi = KpiCalculator.Compute(SalesTotals.Empty);

        Assert.Equal(0m, kpi.Revenue);
        Assert.Null(kpi.Margin);        // не 0% и не NaN
        Assert.Null(kpi.AverageCheck);  // нет деления на ноль
    }

    [Fact]
    public void Selling_below_cost_gives_negative_margin()
    {
        var kpi = KpiCalculator.Compute(new SalesTotals(Revenue: 100m, Cost: 120m, SalesCount: 1));

        Assert.Equal(-20m, kpi.GrossProfit);
        Assert.Equal(-0.2m, kpi.Margin);
    }

    [Theory]
    [InlineData(120, 100, 0.2)]
    [InlineData(80, 100, -0.2)]
    [InlineData(50, -100, 1.5)] // рост из убытка в прибыль — положительная динамика
    public void Relative_change_is_computed_against_absolute_base(double current, double previous, double expected)
    {
        Assert.Equal((decimal)expected, KpiCalculator.RelativeChange((decimal)current, (decimal)previous));
    }

    [Fact]
    public void Relative_change_is_undefined_for_zero_base()
    {
        Assert.Null(KpiCalculator.RelativeChange(100m, 0m));
        Assert.Null(KpiCalculator.RelativeChange(100m, null));
    }

    [Theory]
    [InlineData(SaleStatus.Paid, true)]
    [InlineData(SaleStatus.Cancelled, false)]
    [InlineData(SaleStatus.Refunded, false)]
    public void Only_paid_sales_count_towards_revenue(SaleStatus status, bool expected)
    {
        var rule = SaleRules.CountsTowardsRevenue.Compile();

        Assert.Equal(expected, rule(new Sale { Status = status }));
    }
}
