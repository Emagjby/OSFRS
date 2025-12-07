using FluentAssertions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.Analytics;

public class AnalyticsService_IntegrationTests : IntegrationTestBase
{
    private IAnalyticsService AnalyticsService => Factory.AnalyticsService();
    private IAnalyticsRepository AnalyticsRepo => Factory.AnalyticsRepo();
    private IUsageRepository UsageRepo => Factory.UsageRepo();

    public AnalyticsService_IntegrationTests()
        : base("OSFRS_IT_Analytics_Service") { }

    private async Task SeedUsage(DateTime timestamp, int count = 1, string eventType = "X")
    {
        timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);

        for (int i = 0; i < count; i++)
        {
            await UsageRepo.AddAsync(
                new UsageRecord { EventType = eventType, Timestamp = timestamp }
            );
        }

        await UsageRepo.SaveChangesAsync();
    }

    private static DateTime UtcDate(int year, int month, int day, int hour = 12) =>
        new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Utc);

    // =============================================================
    // DAILY TREND
    // =============================================================

    [Fact]
    public async Task DailyTrends_ShouldReturnEmpty_WhenNoData()
    {
        var start = UtcDate(2025, 1, 1);
        var end = start.AddDays(3);

        var report = await AnalyticsService.GetDailyTrendsAsync(start, end);

        report.Points.Should().BeEmpty();
        report.TotalCount.Should().Be(0);
        report.AveragePerPoint.Should().Be(0);
    }

    [Fact]
    public async Task DailyTrends_ShouldReturnCorrectCounts()
    {
        var day1 = UtcDate(2025, 1, 1);
        var day2 = UtcDate(2025, 1, 2);

        await SeedUsage(day1, 3);
        await SeedUsage(day2, 1);

        var report = await AnalyticsService.GetDailyTrendsAsync(day1, day2);

        report.Points.Should().HaveCount(2);
        report.TotalCount.Should().Be(4);
        report.AveragePerPoint.Should().Be(2);
    }

    [Fact]
    public async Task DailyTrends_ShouldComputeMovingAverage_AndPercentageDiffs()
    {
        var start = UtcDate(2025, 1, 1);

        await SeedUsage(start, 1); // Day 1
        await SeedUsage(start.AddDays(1), 3); // Day 2
        await SeedUsage(start.AddDays(2), 2); // Day 3

        var report = await AnalyticsService.GetDailyTrendsAsync(start, start.AddDays(2));

        report.MovingAverage.Should().NotBeEmpty();
        report.PercentageChange.Should().NotBeEmpty();
        report.TotalCount.Should().Be(6);
    }

    // =============================================================
    // MONTHLY TREND
    // =============================================================

    [Fact]
    public async Task MonthlyTrends_ShouldReturnEmpty_WhenNoData()
    {
        var report = await AnalyticsService.GetMonthlyTrendsAsync(2080);

        report.Points.Should().BeEmpty();
        report.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task MonthlyTrends_ShouldGroupCorrectly()
    {
        await SeedUsage(UtcDate(2025, 1, 10), 2);
        await SeedUsage(UtcDate(2025, 2, 15), 5);
        await SeedUsage(UtcDate(2025, 2, 18), 1);

        var report = await AnalyticsService.GetMonthlyTrendsAsync(2025);

        report.Points.Should().HaveCount(2);
        report.TotalCount.Should().Be(8);
        report.AveragePerPoint.Should().Be(4);
    }

    // =============================================================
    // PEAK USAGE
    // =============================================================

    [Fact]
    public async Task PeakUsage_ShouldReturnZero_WhenNoData()
    {
        var start = UtcDate(2030, 1, 1);
        var end = start.AddDays(7);

        var result = await AnalyticsService.GetPeakUsageAsync(start, end);

        result.PeakCount.Should().Be(0);
        result.PeakTimestamp.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public async Task PeakUsage_ShouldReturnHighestDay()
    {
        var baseDay = UtcDate(2025, 3, 1);

        var target = baseDay.AddDays(1);

        await SeedUsage(baseDay, 1);
        await SeedUsage(target, 4); // peak
        await SeedUsage(baseDay.AddDays(2), 2);

        var result = await AnalyticsService.GetPeakUsageAsync(baseDay, baseDay.AddDays(2));

        result.PeakCount.Should().Be(4);
    }

    // =============================================================
    // VISUALIZATION
    // =============================================================

    [Fact]
    public async Task Visualization_ShouldReturnEmpty_WhenNoData()
    {
        var start = UtcDate(2025, 4, 1);
        var end = start.AddDays(3);

        var vis = await AnalyticsService.GetVisualizationDataAsync(start, end);

        vis.Labels.Should().BeEmpty();
        vis.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task Visualization_ShouldReturnLabelsAndValues()
    {
        var day1 = UtcDate(2025, 4, 1);
        var day2 = day1.AddDays(1);

        await SeedUsage(day1, 1); // "04-01"
        await SeedUsage(day2, 3); // "04-02"

        var vis = await AnalyticsService.GetVisualizationDataAsync(day1, day2);

        vis.Labels.Should().BeEquivalentTo(new[] { "04-01", "04-02" });
        vis.Values.Should().BeEquivalentTo(new[] { 1, 3 });
    }
}
