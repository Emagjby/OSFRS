using FluentAssertions;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.RepositoryTests.Infrastructure;
using OSFRS.RepositoryTests.TestUtils;
using OSFRS.RepositoryTests.TestUtils.EntityBuilders;

namespace OSFRS.RepositoryTests.Repositories;

public class AnalyticsRepositoryTests
{
    private readonly Mock<IAppLogger<AnalyticsRepository>> _logger = new();

    private AnalyticsRepository CreateRepo(TestDbContext db) =>
        new AnalyticsRepository(db.Db, _logger.Object);

    // ------------------------------------------------------------
    // DAILY COUNTS
    // ------------------------------------------------------------
    [Fact]
    public async Task GetDailyCountsAsync_ShouldGroupByDay()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var d1 = DateUtils.At(2025, 1, 1);
        var d2 = DateUtils.At(2025, 1, 2);

        await db.Db.UsageRecords.AddRangeAsync(
            UsageRecordBuilder.Create(timestamp: d1),
            UsageRecordBuilder.Create(timestamp: d1.AddHours(3)),
            UsageRecordBuilder.Create(timestamp: d2),
            UsageRecordBuilder.Create(timestamp: d2.AddHours(5))
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetDailyCountsAsync(d1, d2.AddDays(1))).ToList();

        result.Should().HaveCount(2);

        result[0].Timestamp.Should().Be(d1.Date);
        result[0].Count.Should().Be(2);

        result[1].Timestamp.Should().Be(d2.Date);
        result[1].Count.Should().Be(2);
    }

    [Fact]
    public async Task GetDailyCountsAsync_ShouldRespectInclusiveRange()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var d1 = DateUtils.At(2025, 1, 1);
        var d2 = DateUtils.At(2025, 1, 3);

        await db.Db.UsageRecords.AddRangeAsync(
            UsageRecordBuilder.Create(timestamp: d1),
            UsageRecordBuilder.Create(timestamp: d1.AddHours(2)),
            UsageRecordBuilder.Create(timestamp: d2)
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetDailyCountsAsync(d1, d2)).ToList();

        result.Should().HaveCount(2);

        result.First(r => r.Timestamp == d1.Date).Count.Should().Be(2);
        result.First(r => r.Timestamp == d2.Date).Count.Should().Be(1);
    }

    // ------------------------------------------------------------
    // MONTHLY COUNTS
    // ------------------------------------------------------------
    [Fact]
    public async Task GetMonthlyCountsAsync_ShouldGroupByMonth()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var jan = DateUtils.At(2025, 1, 15);
        var feb = DateUtils.At(2025, 2, 10);

        await db.Db.UsageRecords.AddRangeAsync(
            UsageRecordBuilder.Create(timestamp: jan),
            UsageRecordBuilder.Create(timestamp: jan.AddDays(1)),
            UsageRecordBuilder.Create(timestamp: feb),
            UsageRecordBuilder.Create(timestamp: feb.AddDays(3))
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetMonthlyCountsAsync(2025)).ToList();

        result.Should().HaveCount(2);

        result[0].Timestamp.Should().Be(new DateTime(2025, 1, 1));
        result[0].Count.Should().Be(2);

        result[1].Timestamp.Should().Be(new DateTime(2025, 2, 1));
        result[1].Count.Should().Be(2);
    }

    [Fact]
    public async Task GetMonthlyCountsAsync_ShouldReturnEmpty_WhenNoEvents()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var result = (await repo.GetMonthlyCountsAsync(2033)).ToList();

        result.Should().BeEmpty();
    }

    // ------------------------------------------------------------
    // RAW EVENTS
    // ------------------------------------------------------------
    [Fact]
    public async Task GetRawEventsAsync_ShouldReturnEventsInRange()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var a = DateUtils.At(2025, 3, 1);
        var b = DateUtils.At(2025, 3, 2);
        var c = DateUtils.At(2025, 3, 3);

        await db.Db.UsageRecords.AddRangeAsync(
            UsageRecordBuilder.Create(timestamp: a),
            UsageRecordBuilder.Create(timestamp: b),
            UsageRecordBuilder.Create(timestamp: c)
        );
        await db.Db.SaveChangesAsync();

        var result = (await repo.GetRawEventsAsync(a, b)).ToList();

        result.Should().HaveCount(2);
        result.Select(r => r.Timestamp).Should().Contain(new[] { a, b });
        result.Select(r => r.Timestamp).Should().NotContain(c);
    }

    [Fact]
    public async Task GetRawEventsAsync_ShouldBeNoTracking()
    {
        await using var db = new TestDbContext();
        var repo = CreateRepo(db);

        var t = DateUtils.At(2025, 4, 1);

        await db.Db.UsageRecords.AddAsync(UsageRecordBuilder.Create(timestamp: t));
        await db.Db.SaveChangesAsync();

        db.Db.ChangeTracker.Clear();

        var result = await repo.GetRawEventsAsync(t, t);

        db.Db.ChangeTracker.Entries().Should().BeEmpty();
    }
}
