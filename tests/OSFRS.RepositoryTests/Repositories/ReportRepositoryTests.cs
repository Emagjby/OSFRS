using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Models.Entities;
using OSFRS.RepositoryTests.Infrastructure;
using OSFRS.RepositoryTests.TestUtils;
using OSFRS.RepositoryTests.TestUtils.EntityBuilders;

namespace OSFRS.RepositoryTests.Repositories;

public class ReportRepositoryTests
{
    private readonly Mock<IAppLogger<ReportRepository>> _logger = new();

    private ReportRepository CreateRepo(TestDbContext db) =>
        new ReportRepository(db.Db, _logger.Object);

    // ------------------------------------------------------------
    // DAILY AGGREGATES
    // ------------------------------------------------------------
    [Fact]
    public async Task GetDailyAggregatesAsync_ShouldReturnOnlyDailyAggregatesForThatDay()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var d0 = DateUtils.At(2025, 1, 5);
        var d1 = DateUtils.At(2025, 1, 6);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(eventType: "Login_DailyAggregate", timestamp: d0),
            UsageRecordBuilder.Create(
                eventType: "Action_DailyAggregate",
                timestamp: d0.AddHours(10)
            ),
            UsageRecordBuilder.Create(eventType: "Login_DailyAggregate", timestamp: d1)
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetDailyAggregatesAsync(d0)).ToList();

        result.Should().HaveCount(2);
        result.All(r => r.Timestamp.Date == d0.Date).Should().BeTrue();
        result.All(r => r.EventType.Contains("DailyAggregate")).Should().BeTrue();
    }

    // ------------------------------------------------------------
    // MONTHLY AGGREGATES
    // ------------------------------------------------------------
    [Fact]
    public async Task GetMonthlyAggregatesAsync_ShouldReturnOnlyMonthlyAggregatesForThatMonth()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var jan = DateUtils.At(2025, 1, 1);
        var feb = DateUtils.At(2025, 2, 1);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(eventType: "Login_MonthlyAggregate", timestamp: jan),
            UsageRecordBuilder.Create(
                eventType: "Action_MonthlyAggregate",
                timestamp: jan.AddDays(15)
            ),
            UsageRecordBuilder.Create(eventType: "Login_MonthlyAggregate", timestamp: feb)
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetMonthlyAggregatesAsync(2025, 1)).ToList();

        result.Should().HaveCount(2);
        result.All(r => r.Timestamp.Month == 1 && r.Timestamp.Year == 2025).Should().BeTrue();
        result.All(r => r.EventType.Contains("MonthlyAggregate")).Should().BeTrue();
    }

    // ------------------------------------------------------------
    // REPORT SAVING
    // ------------------------------------------------------------
    [Fact]
    public async Task SaveReportAsync_ShouldPersistReport()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var report = new Report
        {
            Name = "Daily Summary 2025-01-05",
            Type = "Demo",
            GeneratedAt = DateUtils.At(2025, 1, 5),
            AggregatedData = "{}",
        };

        var saved = await repo.SaveReportAsync(report);

        saved.Id.Should().BeGreaterThan(0);
        (await db.Db.Reports.CountAsync()).Should().Be(1);

        var fromDb = await db.Db.Reports.FirstAsync();
        fromDb.Name.Should().Be("Daily Summary 2025-01-05");
        fromDb.Type.Should().Be("Demo");
    }

    [Fact]
    public async Task GetDailyAggregatesAsync_ShouldNotIncludeNonAggregateEvents()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var d0 = DateUtils.At(2025, 1, 10);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(eventType: "Login", timestamp: d0),
            UsageRecordBuilder.Create(
                eventType: "Action_DailyAggregate",
                timestamp: d0.AddHours(2)
            ),
            UsageRecordBuilder.Create(eventType: "Login_DailyAggregate", timestamp: d0.AddHours(1))
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetDailyAggregatesAsync(d0)).ToList();

        result.Should().HaveCount(2);
        result.Any(r => !r.EventType.Contains("DailyAggregate")).Should().BeFalse();
    }

    [Fact]
    public async Task GetMonthlyAggregatesAsync_ShouldOnlyReturnAggregatesEvenIfMixedRecordsExist()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var jan = DateUtils.At(2025, 1, 20);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(eventType: "Login_MonthlyAggregate", timestamp: jan),
            UsageRecordBuilder.Create(eventType: "Action", timestamp: jan.AddDays(5)),
            UsageRecordBuilder.Create(
                eventType: "Action_MonthlyAggregate",
                timestamp: jan.AddDays(10)
            )
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetMonthlyAggregatesAsync(2025, 1)).ToList();

        result.Should().HaveCount(2);
        result.All(r => r.EventType.Contains("MonthlyAggregate")).Should().BeTrue();
    }
}
