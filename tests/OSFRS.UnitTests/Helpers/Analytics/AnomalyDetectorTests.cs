using FluentAssertions;
using OSFRS.Backend.Helpers.Analytics;

namespace OSFRS.UnitTests.Helpers.Analytics;

public class AnomalyDetectorTests
{
    // ============================================================
    // Z–SCORE TESTS
    // ============================================================

    [Fact]
    public void DetectByZScore_ShouldDetectSpike()
    {
        var values = new[] { 10, 12, 11, 80, 12, 11 };

        var result = AnomalyDetector.DetectByZScore(values, threshold: 2.0);

        result.Should().Equal(new[] { 3 }); // 80 is a clear anomaly
    }

    [Fact]
    public void DetectByZScore_ShouldReturnEmpty_OnFlatline()
    {
        var values = new[] { 5, 5, 5, 5, 5 };

        var result = AnomalyDetector.DetectByZScore(values);

        result.Should().BeEmpty(); // std = 0
    }

    [Fact]
    public void DetectByZScore_ShouldReturnEmpty_WhenNotEnoughData()
    {
        var values = new[] { 3, 3 };

        var result = AnomalyDetector.DetectByZScore(values);

        result.Should().BeEmpty();
    }

    [Fact]
    public void DetectByZScore_ShouldHandleNegativeValues()
    {
        var values = new[] { -10, -12, -9, -8, -40, -11 };

        var result = AnomalyDetector.DetectByZScore(values, threshold: 2.0);

        result.Should().Equal(new[] { 4 }); // -40 is the anomaly
    }

    [Fact]
    public void DetectByZScore_ShouldRespectThreshold()
    {
        var values = new[] { 10, 11, 12, 20 };

        var strict = AnomalyDetector.DetectByZScore(values, threshold: 5.0);
        var loose = AnomalyDetector.DetectByZScore(values, threshold: 1.0);

        strict.Should().BeEmpty(); // too strict → no anomalies
        loose.Should().Equal(new[] { 3 }); // 20 stands out under loose threshold
    }

    // ============================================================
    // MAD TESTS
    // ============================================================

    [Fact]
    public void DetectByMAD_ShouldDetectRobustOutlier()
    {
        var values = new[] { 10, 11, 9, 50, 12 };

        var result = AnomalyDetector.DetectByMAD(values);

        result.Should().Equal(new[] { 3 }); // 50 is the spike
    }

    [Fact]
    public void DetectByMAD_ShouldReturnEmpty_OnFlatline()
    {
        var values = new[] { 3, 3, 3, 3 };

        var result = AnomalyDetector.DetectByMAD(values);

        result.Should().BeEmpty(); // MAD = 0 → no anomalies
    }

    [Fact]
    public void DetectByMAD_ShouldReturnEmpty_WhenNotEnoughData()
    {
        var values = new[] { 1, 2 };

        var result = AnomalyDetector.DetectByMAD(values);

        result.Should().BeEmpty();
    }

    [Fact]
    public void DetectByMAD_ShouldDetectSymmetricOutliers()
    {
        var values = new[] { 10, 11, 60, 12, -40 };

        var result = AnomalyDetector.DetectByMAD(values);

        result.Should().Equal(new[] { 2, 4 }); // both extremes flagged
    }

    [Fact]
    public void DetectByMAD_ShouldRespectThreshold()
    {
        var values = new[] { 10, 11, 12, 30 };

        var strict = AnomalyDetector.DetectByMAD(values, threshold: 13.0);
        var loose = AnomalyDetector.DetectByMAD(values, threshold: 2.0);

        strict.Should().BeEmpty();
        loose.Should().Equal(new[] { 3 });
    }
}
