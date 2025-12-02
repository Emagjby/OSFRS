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

public class UsageRepositoryTests
{
    private readonly Mock<IAppLogger<BaseRepository<UsageRecord>>> _logger = new();

    private UsageRepository CreateRepo(TestDbContext db) =>
        new UsageRepository(db.Db, _logger.Object);

    // ------------------------------------------------------------
    // BASIC ADDING THROUGH BASE REPO
    // ------------------------------------------------------------
    [Fact]
    public async Task AddAsync_ShouldAddRecord()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var record = UsageRecordBuilder.Create(eventType: "Login");

        await repo.AddAsync(record);
        await repo.SaveChangesAsync();

        (await db.Db.UsageRecords.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleRecords()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var r1 = UsageRecordBuilder.Create(eventType: "A");
        var r2 = UsageRecordBuilder.Create(eventType: "B");

        await repo.AddRangeAsync(new[] { r1, r2 });
        await repo.SaveChangesAsync();

        (await db.Db.UsageRecords.CountAsync()).Should().Be(2);
    }

    // ------------------------------------------------------------
    // DAILY ANALYTICS
    // ------------------------------------------------------------
    [Fact]
    public async Task GetDailyAnalyticsAsync_ShouldReturnOnlyRecordsFromThatDay()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var d0 = DateUtils.At(2025, 1, 5);
        var d1 = DateUtils.At(2025, 1, 6);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(timestamp: d0.AddHours(3)),
            UsageRecordBuilder.Create(timestamp: d0.AddHours(7)),
            UsageRecordBuilder.Create(timestamp: d1.AddHours(1))
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetDailyAnalyticsAsync(d0)).ToList();

        result.Should().HaveCount(2);
        result.All(r => r.Timestamp.Date == d0.Date).Should().BeTrue();
    }

    // ------------------------------------------------------------
    // MONTHLY ANALYTICS
    // ------------------------------------------------------------
    [Fact]
    public async Task GetMonthlyAnalyticsAsync_ShouldReturnOnlyRecordsFromThatMonth()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var jan = DateUtils.At(2025, 1, 1);
        var feb = DateUtils.At(2025, 2, 1);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(timestamp: jan.AddDays(2)),
            UsageRecordBuilder.Create(timestamp: jan.AddDays(15)),
            UsageRecordBuilder.Create(timestamp: feb.AddDays(3))
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetMonthlyAnalyticsAsync(2025, 1)).ToList();

        result.Should().HaveCount(2);
    }

    // ------------------------------------------------------------
    // QUERY FILTERS
    // ------------------------------------------------------------

    [Fact]
    public async Task QueryAsync_ShouldFilterByEventType()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(eventType: "A"),
            UsageRecordBuilder.Create(eventType: "B"),
            UsageRecordBuilder.Create(eventType: "A")
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.QueryAsync(eventType: "A")).ToList();

        result.Should().HaveCount(2);
        result.All(u => u.EventType == "A").Should().BeTrue();
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByUserId()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(userId: 1),
            UsageRecordBuilder.Create(userId: 2),
            UsageRecordBuilder.Create(userId: 1)
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.QueryAsync(userId: 1)).ToList();

        result.Should().HaveCount(2);
        result.All(u => u.UserId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByFacilityId()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(facilityId: 10),
            UsageRecordBuilder.Create(facilityId: 20),
            UsageRecordBuilder.Create(facilityId: 10)
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.QueryAsync(facilityId: 10)).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByTimestampRange()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var t0 = DateUtils.UtcNow;
        var t1 = t0.AddHours(1);
        var t2 = t0.AddHours(2);

        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(timestamp: t0),
            UsageRecordBuilder.Create(timestamp: t1),
            UsageRecordBuilder.Create(timestamp: t2)
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.QueryAsync(start: t1, end: t2)).ToList();

        result.Should().HaveCount(2);
        result.First().Timestamp.Should().Be(t1);
        result.Last().Timestamp.Should().Be(t2);
    }

    [Fact]
    public async Task QueryAsync_ShouldOrderByTimestampAscending()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var t0 = DateUtils.UtcNow;
        await db.Db.AddRangeAsync(
            UsageRecordBuilder.Create(timestamp: t0.AddHours(2)),
            UsageRecordBuilder.Create(timestamp: t0.AddHours(1)),
            UsageRecordBuilder.Create(timestamp: t0.AddHours(3))
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.QueryAsync()).ToList();

        result.Should().BeInAscendingOrder(r => r.Timestamp);
    }
}
