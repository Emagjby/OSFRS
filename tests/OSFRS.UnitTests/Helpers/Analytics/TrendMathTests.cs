using FluentAssertions;
using OSFRS.Backend.Helpers.Analytics;

namespace OSFRS.UnitTests.Helpers;

public class TrendMathTests
{
    // ============================================================
    // MOVING AVERAGE
    // ============================================================

    [Fact]
    public void MovingAverage_ShouldComputeCorrectly()
    {
        var values = new[] { 1, 2, 3, 4, 5 };

        var result = TrendMath.MovingAverage(values, 3);

        result.Should().Equal(new double[] { 2, 2, 2, 3, 4 });
    }

    [Fact]
    public void MovingAverage_ShouldReturnOriginal_WhenWindowInvalid()
    {
        var values = new[] { 5, 10, 15 };

        TrendMath.MovingAverage(values, 1).Should().Equal(values.Select(x => (double)x));

        TrendMath.MovingAverage(values, 10).Should().Equal(values.Select(x => (double)x));
    }

    [Fact]
    public void MovingAverage_ShouldHandleNegativeValues()
    {
        var values = new[] { -2, -4, -6 };

        var result = TrendMath.MovingAverage(values, 2);

        result.Should().Equal(new double[] { -3, -3, -5 });
    }

    [Fact]
    public void MovingAverage_ShouldHandleSingleValue()
    {
        var result = TrendMath.MovingAverage(new[] { 100 }, 3);

        result.Should().Equal(new double[] { 100 });
    }

    // ============================================================
    // PERCENTAGE CHANGES
    // ============================================================

    [Fact]
    public void PercentageChanges_ShouldComputeCorrectly()
    {
        var result = TrendMath.PercentageChanges(new[] { 10, 20, 40 });

        result.Should().Equal(new double[] { 100, 100 });
    }

    [Fact]
    public void PercentageChanges_ShouldReturnZero_WhenPrevZero()
    {
        var result = TrendMath.PercentageChanges(new[] { 0, 50, 100 });

        result.Should().Equal(new double[] { 0, 100 });
    }

    [Fact]
    public void PercentageChanges_ShouldHandleNegativeValues()
    {
        var result = TrendMath.PercentageChanges(new[] { -10, -20 });

        result.Should().Equal(new double[] { 100 });
    }

    [Fact]
    public void PercentageChanges_ShouldBeEmpty_WithSingleValue()
    {
        var result = TrendMath.PercentageChanges(new[] { 50 });

        result.Should().BeEmpty();
    }

    // ============================================================
    // GROWTH RATE
    // ============================================================

    [Fact]
    public void GrowthRate_ShouldComputeCorrectly()
    {
        TrendMath.GrowthRate(new[] { 10, 20 }).Should().Be(100);
    }

    [Fact]
    public void GrowthRate_ShouldReturnZero_WhenStartZero()
    {
        TrendMath.GrowthRate(new[] { 0, 50 }).Should().Be(0);
    }

    [Fact]
    public void GrowthRate_ShouldHandleNegativeValues()
    {
        TrendMath.GrowthRate(new[] { -50, -100 }).Should().Be(100);
    }

    [Fact]
    public void GrowthRate_ShouldReturnZero_WhenSingleValue()
    {
        TrendMath.GrowthRate(new[] { 50 }).Should().Be(0);
    }
}
