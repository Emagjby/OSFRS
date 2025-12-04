using FluentAssertions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.Usage;

public class UsageRepository_IntegrationTests : IntegrationTestBase
{
    public UsageRepository_IntegrationTests()
        : base("OSFRS_IT_UsageRepository") { }

    private IUsageRepository Repo() => Factory.UsageRepo();

    private UsageRecord Build(
        string type,
        int? user = null,
        int? fac = null,
        DateTime? ts = null
    ) =>
        new UsageRecord
        {
            EventType = type,
            UserId = user,
            FacilityId = fac,
            Timestamp = ts ?? DateTime.UtcNow,
        };

    // --------------------------------------------------------------------
    // 1. ADD
    // --------------------------------------------------------------------
    [Fact]
    public async Task AddAsync_ShouldInsertRecord()
    {
        var r = Build("ReservationCreated", 5, 2);

        await Repo().AddAsync(r);
        await Repo().SaveChangesAsync();

        var result = await Repo().QueryAsync();

        result.Should().HaveCount(1);
        result.First().EventType.Should().Be("ReservationCreated");
    }

    // --------------------------------------------------------------------
    // 2. ADD RANGE
    // --------------------------------------------------------------------
    [Fact]
    public async Task AddRangeAsync_ShouldInsertMultipleRecords()
    {
        var records = new[] { Build("A", 1, 1), Build("B", 2, 1), Build("C", 3, 2) };

        await Repo().AddRangeAsync(records);
        await Repo().SaveChangesAsync();

        var all = await Repo().QueryAsync();

        all.Should().HaveCount(3);
    }

    // --------------------------------------------------------------------
    // 3–8 QUERY FILTERS
    // --------------------------------------------------------------------
    [Fact]
    public async Task Query_ShouldFilterByEventType()
    {
        await Repo().AddAsync(Build("X"));
        await Repo().AddAsync(Build("Y"));
        await Repo().SaveChangesAsync();

        var result = await Repo().QueryAsync(eventType: "X");

        result.Should().HaveCount(1);
        result.First().EventType.Should().Be("X");
    }

    [Fact]
    public async Task Query_ShouldFilterByUserId()
    {
        await Repo().AddAsync(Build("E", user: 5));
        await Repo().AddAsync(Build("E", user: 10));
        await Repo().SaveChangesAsync();

        var result = await Repo().QueryAsync(userId: 5);

        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(5);
    }

    [Fact]
    public async Task Query_ShouldFilterByFacilityId()
    {
        await Repo().AddAsync(Build("E", fac: 2));
        await Repo().AddAsync(Build("E", fac: 99));
        await Repo().SaveChangesAsync();

        var result = await Repo().QueryAsync(facilityId: 2);

        result.Should().HaveCount(1);
        result.First().FacilityId.Should().Be(2);
    }

    [Fact]
    public async Task Query_ShouldFilterByStartRange()
    {
        var t1 = DateTime.UtcNow.AddHours(-2);
        var t2 = DateTime.UtcNow.AddHours(-1);

        await Repo().AddAsync(Build("E", ts: t1));
        await Repo().AddAsync(Build("E", ts: t2));
        await Repo().SaveChangesAsync();

        var result = await Repo().QueryAsync(start: t2);

        result.Should().HaveCount(1);
        result.First().Timestamp.Should().Be(t2);
    }

    [Fact]
    public async Task Query_ShouldFilterByEndRange()
    {
        var t1 = DateTime.UtcNow.AddHours(-2);
        var t2 = DateTime.UtcNow.AddHours(1);

        await Repo().AddAsync(Build("E", ts: t1));
        await Repo().AddAsync(Build("E", ts: t2));
        await Repo().SaveChangesAsync();

        var result = await Repo().QueryAsync(end: DateTime.UtcNow);

        result.Should().HaveCount(1);
        result.First().Timestamp.Should().Be(t1);
    }

    // --------------------------------------------------------------------
    // 9. DAILY AGGREGATION
    // --------------------------------------------------------------------
    [Fact]
    public async Task AggregateDaily_ShouldGroupRawEventsCorrectly()
    {
        var day = DateTime.UtcNow.Date;

        await Repo().AddAsync(Build("ReservationCreated", 1, 1, day.AddHours(2)));
        await Repo().AddAsync(Build("ReservationCreated", 1, 1, day.AddHours(3)));
        await Repo().AddAsync(Build("FacilityUpdated", 1, 1, day.AddHours(4)));

        await Repo().SaveChangesAsync();

        var agg = await Repo().AggregateDailyAsync(day);

        agg.Should().HaveCount(2);

        agg.Any(a => a.EventType == "ReservationCreated_DailyAggregate").Should().BeTrue();
        agg.Any(a => a.EventType == "FacilityUpdated_DailyAggregate").Should().BeTrue();
    }

    // --------------------------------------------------------------------
    // 10. DAILY REPLACEMENT
    // --------------------------------------------------------------------
    [Fact]
    public async Task AggregateDaily_ShouldReplaceOldAggregates()
    {
        var day = DateTime.UtcNow.Date;

        // First aggregation
        await Repo().AddAsync(Build("X", 1, 1, day.AddHours(2)));
        await Repo().SaveChangesAsync();

        var first = await Repo().AggregateDailyAsync(day);
        await Repo().SaveChangesAsync();

        first.Should().HaveCount(1);

        // Replace it with new events
        await Repo().AddAsync(Build("X", 1, 1, day.AddHours(3)));
        await Repo().SaveChangesAsync();

        var newAgg = await Repo().AggregateDailyAsync(day);

        newAgg.Should().HaveCount(1);
        newAgg.First().EventType.Should().Contain("X");
    }

    // --------------------------------------------------------------------
    // 11. MONTHLY AGGREGATION
    // --------------------------------------------------------------------
    [Fact]
    public async Task AggregateMonthly_ShouldGroupByMonth()
    {
        var now = DateTime.UtcNow;
        int y = now.Year;
        int m = now.Month;

        var monthStart = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);

        await Repo().AddAsync(Build("ReservationCreated", 5, 2, monthStart.AddDays(1)));
        await Repo().AddAsync(Build("ReservationCreated", 5, 2, monthStart.AddDays(2)));
        await Repo().AddAsync(Build("FacilityUpdated", 5, 2, monthStart.AddDays(3)));

        await Repo().SaveChangesAsync();

        var agg = await Repo().AggregateMonthlyAsync(y, m);

        agg.Should().HaveCount(2);
        agg.Any(a => a.EventType == "ReservationCreated_MonthlyAggregate").Should().BeTrue();
        agg.Any(a => a.EventType == "FacilityUpdated_MonthlyAggregate").Should().BeTrue();
    }

    // --------------------------------------------------------------------
    // 12. MONTHLY REPLACEMENT
    // --------------------------------------------------------------------
    [Fact]
    public async Task AggregateMonthly_ShouldReplaceExisting()
    {
        var now = DateTime.UtcNow;
        int y = now.Year;
        int m = now.Month;

        // First aggregate
        await Repo().AddAsync(Build("A", 1, 1, now));
        await Repo().SaveChangesAsync();
        await Repo().AggregateMonthlyAsync(y, m);
        await Repo().SaveChangesAsync();

        // New raw event
        await Repo().AddAsync(Build("A", 1, 1, now));
        await Repo().SaveChangesAsync();

        var agg = await Repo().AggregateMonthlyAsync(y, m);

        agg.Should().ContainSingle(a => a.EventType.Contains("A"));
    }

    // --------------------------------------------------------------------
    // 13. EMPTY AGGREGATION
    // --------------------------------------------------------------------
    [Fact]
    public async Task AggregateDaily_ShouldReturnEmpty_WhenNoRawEvents()
    {
        var day = DateTime.UtcNow.Date;

        var result = await Repo().AggregateDailyAsync(day);

        result.Should().BeEmpty();
    }

    // --------------------------------------------------------------------
    // 14–15 DAILY & MONTHLY ANALYTICS GETTERS
    // --------------------------------------------------------------------
    [Fact]
    public async Task GetDailyAnalytics_ShouldReturnEventsWithinDay()
    {
        var day = DateTime.UtcNow.Date;

        await Repo().AddAsync(Build("A", ts: day.AddHours(1)));
        await Repo().AddAsync(Build("B", ts: day.AddHours(23)));
        await Repo().AddAsync(Build("C", ts: day.AddDays(1))); // out of range

        await Repo().SaveChangesAsync();

        var records = await Repo().GetDailyAnalyticsAsync(day);

        records.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMonthlyAnalytics_ShouldReturnEventsInMonth()
    {
        var now = DateTime.UtcNow;
        int y = now.Year;
        int m = now.Month;

        var monthStart = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);

        await Repo().AddAsync(Build("A", ts: monthStart.AddDays(1)));
        await Repo().AddAsync(Build("B", ts: monthStart.AddDays(10)));
        await Repo().AddAsync(Build("C", ts: monthStart.AddMonths(1))); // out of range

        await Repo().SaveChangesAsync();

        var records = await Repo().GetMonthlyAnalyticsAsync(y, m);

        records.Should().HaveCount(2);
    }
}
