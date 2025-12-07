using FluentAssertions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.Analytics;

public class AnalyticsRepository_IntegrationTests : IntegrationTestBase
{
    private IAnalyticsRepository A => Factory.AnalyticsRepo();
    private IUsageRepository U => Factory.UsageRepo();

    public AnalyticsRepository_IntegrationTests()
        : base("OSFRS_IT_Analytics_Repo") { }

    private async Task Seed(DateTime ts, string et = "X", int? u = 1, int? f = 1)
    {
        await U.AddAsync(
            new UsageRecord
            {
                EventType = et,
                UserId = u,
                FacilityId = f,
                Timestamp = ts,
            }
        );
        await U.SaveChangesAsync();
    }

    // =============================================================
    // DAILY COUNTS
    // =============================================================

    [Fact]
    public async Task GetDailyCounts_ShouldGroupByDay()
    {
        var day = new DateTime(2025, 12, 4, 0, 0, 0, DateTimeKind.Utc);

        await Seed(day.AddHours(1));
        await Seed(day.AddHours(2));

        // IMPORTANT: inclusive range → to must be +1 day
        var result = await A.GetDailyCountsAsync(day, day.AddDays(1));

        result.Should().HaveCount(1);
        result.First().Count.Should().Be(2);
        result.First().Timestamp.Should().Be(day);
    }

    [Fact]
    public async Task GetDailyCounts_ShouldReturnMultipleDays()
    {
        var d1 = new DateTime(2025, 12, 4, 0, 0, 0, DateTimeKind.Utc);
        var d2 = d1.AddDays(1);

        await Seed(d1.AddHours(2));
        await Seed(d2.AddHours(5));

        var result = await A.GetDailyCountsAsync(d1, d2.AddDays(1));

        result.Should().HaveCount(2);
        result.Select(x => x.Timestamp).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetDailyCounts_ShouldRespectRange()
    {
        var day = new DateTime(2025, 12, 4, 0, 0, 0, DateTimeKind.Utc);

        await Seed(day.AddDays(-1)); // OUT
        await Seed(day); // IN (exact start)
        await Seed(day.AddDays(2)); // OUT if we use range [day, day]

        var result = await A.GetDailyCountsAsync(day, day.AddDays(1));

        result.Should().HaveCount(1);
        result.First().Count.Should().Be(1);
    }

    [Fact]
    public async Task GetDailyCounts_ShouldReturnEmpty_WhenNoEvents()
    {
        var d = new DateTime(2025, 12, 10, 0, 0, 0, DateTimeKind.Utc);
        var res = await A.GetDailyCountsAsync(d, d.AddDays(1));
        res.Should().BeEmpty();
    }

    // =============================================================
    // MONTHLY COUNTS
    // =============================================================

    [Fact]
    public async Task GetMonthlyCounts_ShouldGroupByMonth()
    {
        var jan = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var feb = new DateTime(2025, 2, 5, 13, 0, 0, DateTimeKind.Utc);

        await Seed(jan);
        await Seed(jan.AddDays(1));
        await Seed(feb);

        var res = await A.GetMonthlyCountsAsync(2025);

        res.Should().HaveCount(2);
        res.Select(r => r.Timestamp).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetMonthlyCounts_ShouldOnlyIncludeSpecifiedYear()
    {
        var prev = new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        var thisYear = new DateTime(2025, 5, 10, 0, 0, 0, DateTimeKind.Utc);

        await Seed(prev);
        await Seed(thisYear);

        var res = await A.GetMonthlyCountsAsync(2025);

        res.Should().HaveCount(1);
        res.First().Timestamp.Year.Should().Be(2025);
    }

    [Fact]
    public async Task GetMonthlyCounts_ShouldReturnEmpty_WhenNoData()
    {
        var targetYear = 2050;

        var res = await A.GetMonthlyCountsAsync(targetYear);

        res.Should().BeEmpty();
    }

    // =============================================================
    // RAW EVENTS
    // =============================================================

    [Fact]
    public async Task GetRawEvents_ShouldReturnInsideRangeOnly()
    {
        var t = new DateTime(2025, 4, 1, 12, 0, 0, DateTimeKind.Utc);

        await Seed(t.AddDays(-1)); // out
        await Seed(t); // in
        await Seed(t.AddDays(1)); // out

        var res = await A.GetRawEventsAsync(t, t);

        res.Should().HaveCount(1);
        res.First().Timestamp.Should().Be(t);
    }

    [Fact]
    public async Task GetRawEvents_ShouldBeNoTracking()
    {
        var t = new DateTime(2025, 4, 1, 12, 0, 0, DateTimeKind.Utc);

        await Seed(t);

        var res = await A.GetRawEventsAsync(t, t);

        // modify returned entity
        var rec = res.First();
        rec.EventType = "CHANGED";

        // re-read from DB
        var reread = await A.GetRawEventsAsync(t, t);

        reread.First().EventType.Should().NotBe("CHANGED");
    }
}
