using FluentAssertions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.Reports;

public class ReportRepository_IntegrationTests : IntegrationTestBase
{
    private IReportRepository Repo => Factory.ReportRepo();
    private IUsageRepository Usage => Factory.UsageRepo();

    public ReportRepository_IntegrationTests()
        : base("OSFRS_IT_Reports_Repo") { }

    private async Task SeedDailyAggregate(DateTime ts, string et = "X_DailyAggregate")
    {
        await Usage.AddAsync(new UsageRecord { EventType = et, Timestamp = ts });
        await Usage.SaveChangesAsync();
    }

    private async Task SeedMonthlyAggregate(DateTime ts, string et = "X_MonthlyAggregate")
    {
        await Usage.AddAsync(new UsageRecord { EventType = et, Timestamp = ts });
        await Usage.SaveChangesAsync();
    }

    private async Task SeedNonAggregate(DateTime ts, string et = "X")
    {
        await Usage.AddAsync(new UsageRecord { EventType = et, Timestamp = ts });
        await Usage.SaveChangesAsync();
    }

    // =============================================================
    // DAILY AGGREGATES
    // =============================================================
    [Fact]
    public async Task GetDailyAggregates_ShouldReturnDailyOnly()
    {
        var day = DateTime.UtcNow.Date;

        await SeedDailyAggregate(day.AddHours(1));
        await SeedDailyAggregate(day.AddHours(2));
        await SeedNonAggregate(day.AddHours(3));

        var res = await Repo.GetDailyAggregatesAsync(day);

        res.Should().HaveCount(2);
        res.All(r => r.EventType.Contains("DailyAggregate")).Should().BeTrue();
    }

    [Fact]
    public async Task GetDailyAggregates_ShouldRespectDate()
    {
        var day = DateTime.UtcNow.Date;

        await SeedDailyAggregate(day.AddDays(-1)); // out
        await SeedDailyAggregate(day); // in
        await SeedDailyAggregate(day.AddDays(1)); // out

        var res = await Repo.GetDailyAggregatesAsync(day);

        res.Should().HaveCount(1);
        res.First().Timestamp.Should().Be(day);
    }

    [Fact]
    public async Task GetDailyAggregates_ShouldReturnEmpty_WhenNone()
    {
        var day = DateTime.UtcNow.Date;

        var res = await Repo.GetDailyAggregatesAsync(day);
        res.Should().BeEmpty();
    }

    // =============================================================
    // MONTHLY AGGREGATES
    // =============================================================
    [Fact]
    public async Task GetMonthlyAggregates_ShouldReturnMonthlyOnly()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        await SeedMonthlyAggregate(monthStart.AddDays(1));
        await SeedMonthlyAggregate(monthStart.AddDays(2));
        await SeedNonAggregate(monthStart.AddDays(3));

        var res = await Repo.GetMonthlyAggregatesAsync(now.Year, now.Month);

        res.Should().HaveCount(2);
        res.All(r => r.EventType.Contains("MonthlyAggregate")).Should().BeTrue();
    }

    [Fact]
    public async Task GetMonthlyAggregates_ShouldRespectYearAndMonth()
    {
        var now = DateTime.UtcNow;

        await SeedMonthlyAggregate(new DateTime(now.Year, now.Month - 1, 2)); // in
        await SeedMonthlyAggregate(new DateTime(now.Year, now.Month - 1, 10)); // in
        await SeedMonthlyAggregate(new DateTime(now.Year, now.Month, 1)); // out
        await SeedMonthlyAggregate(new DateTime(now.Year - 1, now.Month - 1, 1)); // out

        var res = await Repo.GetMonthlyAggregatesAsync(now.Year, now.Month - 1);

        res.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMonthlyAggregates_ShouldReturnEmpty_WhenNone()
    {
        var now = DateTime.UtcNow;

        var res = await Repo.GetMonthlyAggregatesAsync(now.Year + 5, now.Month);
        res.Should().BeEmpty();
    }

    // =============================================================
    // SAVE REPORT
    // =============================================================
    [Fact]
    public async Task SaveReport_ShouldPersistEntity()
    {
        var report = new Report
        {
            Name = "MyReport",
            Type = "Test",
            AggregatedData = "Test content",
            GeneratedAt = DateTime.UtcNow,
        };

        var saved = await Repo.SaveReportAsync(report);

        saved.Id.Should().BeGreaterThan(0);

        // verify from db
        var verify = DbContext.Db.Reports.FirstOrDefault(r => r.Id == saved.Id);

        verify.Should().NotBeNull();
        verify!.Name.Should().Be("MyReport");
        verify.AggregatedData.Should().Be("Test content");
    }
}
