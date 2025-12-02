using FluentAssertions;
using Moq;
using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Services;
using OSFRS.UnitTests.TestUtils;

namespace OSFRS.UnitTests.Services;

public class AnalyticsServiceTests
{
    private readonly Mock<IAnalyticsRepository> _repo;
    private readonly Mock<IAppLogger<AnalyticsService>> _logger;

    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        _repo = MockFactories.AnalyticsRepo();
        _logger = MockFactories.Logger<AnalyticsService>();

        _service = new AnalyticsService(_repo.Object, _logger.Object);
    }

    // ============================================================
    // ANOMALY DETECTION
    // ============================================================

    [Fact]
    public async Task DetectAnomalies_ShouldReturnMADResults()
    {
        var data = FakeData.SimpleDaily(1, 2, 50, 2, 1);

        _repo
            .Setup(r => r.GetDailyCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(data);

        var result = await _service.DetectAnomaliesAsync(
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow,
            "mad"
        );

        result.Anomalies.Should().NotBeEmpty();
        result.DetectionMode.Should().Be("mad");
    }

    [Fact]
    public async Task DetectAnomalies_ShouldThrow_WhenInvalidMode()
    {
        var data = FakeData.SimpleDaily(1, 2, 3);

        _repo
            .Setup(r => r.GetDailyCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(data);

        var act = async () =>
            await _service.DetectAnomaliesAsync(DateTime.UtcNow, DateTime.UtcNow, "invalid");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ============================================================
    // DAILY TREND REPORT
    // ============================================================

    [Fact]
    public async Task GetDailyTrends_ShouldComputeTotals_AndAverages()
    {
        var data = FakeData.SimpleDaily(2, 4, 6); // total = 12, avg = 4

        _repo
            .Setup(r => r.GetDailyCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(data);

        var result = await _service.GetDailyTrendsAsync(DateTime.UtcNow, DateTime.UtcNow);

        result.TotalCount.Should().Be(12);
        result.AveragePerPoint.Should().Be(4);
        result.Points.Count().Should().Be(3);
    }

    [Fact]
    public async Task GetDailyTrends_ShouldHandleEmptyData()
    {
        _repo
            .Setup(r => r.GetDailyCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrendPointDto>());

        var result = await _service.GetDailyTrendsAsync(DateTime.UtcNow, DateTime.UtcNow);

        result.TotalCount.Should().Be(0);
        result.AveragePerPoint.Should().Be(0);
        result.Points.Should().BeEmpty();
    }

    // ============================================================
    // MONTHLY TREND REPORT
    // ============================================================

    [Fact]
    public async Task GetMonthlyTrends_ShouldAggregateCounts()
    {
        var data = FakeData.SimpleDaily(10, 20, 30);

        _repo.Setup(r => r.GetMonthlyCountsAsync(2024)).ReturnsAsync(data);

        var result = await _service.GetMonthlyTrendsAsync(2024);

        result.TotalCount.Should().Be(60);
        result.AveragePerPoint.Should().Be(20);
    }

    [Fact]
    public async Task GetMonthlyTrends_ShouldHandleEmpty()
    {
        _repo.Setup(r => r.GetMonthlyCountsAsync(2024)).ReturnsAsync(new List<TrendPointDto>());

        var result = await _service.GetMonthlyTrendsAsync(2024);

        result.TotalCount.Should().Be(0);
        result.Points.Should().BeEmpty();
    }

    // ============================================================
    // PEAK USAGE
    // ============================================================

    [Fact]
    public async Task GetPeakUsage_ShouldReturnMaxPoint()
    {
        var data = FakeData.SimpleDaily(5, 10, 3);

        _repo
            .Setup(r => r.GetDailyCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(data);

        var result = await _service.GetPeakUsageAsync(DateTime.UtcNow, DateTime.UtcNow);

        result.PeakCount.Should().Be(10);
    }

    [Fact]
    public async Task GetPeakUsage_ShouldReturnDefaults_WhenEmpty()
    {
        _repo
            .Setup(r => r.GetDailyCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrendPointDto>());

        var result = await _service.GetPeakUsageAsync(DateTime.UtcNow, DateTime.UtcNow);

        result.PeakCount.Should().Be(0);
        result.PeakTimestamp.Should().Be(DateTime.MinValue);
    }

    // ============================================================
    // VISUALIZATION DATA
    // ============================================================

    [Fact]
    public async Task GetVisualizationData_ShouldReturnLabelsAndValues()
    {
        var data = FakeData.SimpleDaily(3, 5, 7);

        _repo
            .Setup(r => r.GetDailyCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(data);

        var result = await _service.GetVisualizationDataAsync(DateTime.UtcNow, DateTime.UtcNow);

        result.Values.Should().BeEquivalentTo(new[] { 3, 5, 7 });
        result.Labels.Should().HaveCount(3);
        result.ChartType.Should().Be("line");
    }

    [Fact]
    public async Task GetVisualizationData_ShouldReturnEmpty_WhenNoData()
    {
        _repo
            .Setup(r => r.GetDailyCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrendPointDto>());

        var result = await _service.GetVisualizationDataAsync(DateTime.UtcNow, DateTime.UtcNow);

        result.Labels.Should().BeEmpty();
        result.Values.Should().BeEmpty();
    }
}
