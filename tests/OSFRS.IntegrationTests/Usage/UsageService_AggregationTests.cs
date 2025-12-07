using FluentAssertions;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.IntegrationTests.Infrastructure;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.Usage;

public class UsageRepository_AggregationTests : IntegrationTestBase
{
    private IUsageRepository Repo() => Factory.UsageRepo();

    public UsageRepository_AggregationTests()
        : base("OSFRS_IT_Usage_Aggregation") { }

    private async Task SeedRaw(DateTime ts, string type = "X", int user = 1, int fac = 1)
    {
        await Repo()
            .AddAsync(
                new UsageRecord
                {
                    EventType = type,
                    UserId = user,
                    FacilityId = fac,
                    Timestamp = ts,
                }
            );

        await Repo().SaveChangesAsync();
    }

    // =============================================================
    // DAILY AGGREGATION
    // =============================================================

    [Fact]
    public async Task DailyAggregate_ShouldAggregateRawEvents()
    {
        var today = DateTime.UtcNow.Date;

        await SeedRaw(today.AddHours(1), "X");
        await SeedRaw(today.AddHours(2), "X");
        await SeedRaw(today.AddHours(3), "X");
        await SeedRaw(today.AddHours(4), "Y");
        await SeedRaw(today.AddHours(5), "Y");

        var result = await Repo().AggregateDailyAsync(today);

        result.Should().HaveCount(2);

        result
            .Should()
            .Contain(r => r.EventType == "X_DailyAggregate" && r.AggregatedData == "Count=3");

        result
            .Should()
            .Contain(r => r.EventType == "Y_DailyAggregate" && r.AggregatedData == "Count=2");
    }

    [Fact]
    public async Task DailyAggregate_ShouldIgnoreExistingAggregates()
    {
        var today = DateTime.UtcNow.Date;

        await SeedRaw(today.AddHours(2), "X");

        // existing aggregate — must be ignored
        await Repo()
            .AddAsync(
                new UsageRecord
                {
                    EventType = "X_DailyAggregate",
                    UserId = 1,
                    FacilityId = 1,
                    Timestamp = today,
                }
            );
        await Repo().SaveChangesAsync();

        var result = await Repo().AggregateDailyAsync(today);

        result.Should().HaveCount(1);
        result.Single().AggregatedData.Should().Be("Count=1");
    }

    [Fact]
    public async Task DailyAggregate_ShouldReplaceOldAggregates()
    {
        var today = DateTime.UtcNow.Date;

        // raw
        await SeedRaw(today.AddHours(1), "X");

        // stale old aggregate
        await Repo()
            .AddAsync(
                new UsageRecord
                {
                    EventType = "X_DailyAggregate",
                    UserId = 1,
                    FacilityId = 1,
                    Timestamp = today,
                    AggregatedData = "Count=99",
                }
            );
        await Repo().SaveChangesAsync();

        var result = await Repo().AggregateDailyAsync(today);

        result.Should().HaveCount(1);
        result.Single().AggregatedData.Should().Be("Count=1");
    }

    [Fact]
    public async Task DailyAggregate_ShouldReturnEmpty_WhenNoRawEvents()
    {
        var today = DateTime.UtcNow.Date;

        var result = await Repo().AggregateDailyAsync(today);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DailyAggregate_ShouldGroupByEventTypeUserFacility()
    {
        var today = DateTime.UtcNow.Date;

        await SeedRaw(today.AddHours(1), "X", user: 1, fac: 1);
        await SeedRaw(today.AddHours(2), "X", user: 1, fac: 2);
        await SeedRaw(today.AddHours(3), "X", user: 2, fac: 1);

        var result = await Repo().AggregateDailyAsync(today);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task DailyAggregate_TimestampShouldEqualStartOfDay()
    {
        var today = DateTime.UtcNow.Date;

        await SeedRaw(today.AddHours(1), "X");

        var result = await Repo().AggregateDailyAsync(today);

        result.Single().Timestamp.Should().Be(today);
    }

    // =============================================================
    // MONTHLY AGGREGATION
    // =============================================================

    [Fact]
    public async Task MonthlyAggregate_ShouldAggregateCorrectly()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        await SeedRaw(monthStart.AddDays(1), "X");
        await SeedRaw(monthStart.AddDays(2), "X");
        await SeedRaw(monthStart.AddDays(3), "Y");

        var result = await Repo().AggregateMonthlyAsync(now.Year, now.Month);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task MonthlyAggregate_ShouldIgnoreExistingAggregates()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        await SeedRaw(monthStart.AddDays(1), "X");

        await Repo()
            .AddAsync(
                new UsageRecord
                {
                    EventType = "X_MonthlyAggregate",
                    UserId = 1,
                    FacilityId = 1,
                    Timestamp = monthStart,
                    AggregatedData = "Count=999",
                }
            );
        await Repo().SaveChangesAsync();

        var result = await Repo().AggregateMonthlyAsync(now.Year, now.Month);

        result.Should().HaveCount(1);
        result.Single().AggregatedData.Should().Be("Count=1");
    }

    [Fact]
    public async Task MonthlyAggregate_ShouldReturnEmpty_WhenNoRawEvents()
    {
        var now = DateTime.UtcNow;

        var result = await Repo().AggregateMonthlyAsync(now.Year, now.Month);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task MonthlyAggregate_TimestampShouldBeMonthStart()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        await SeedRaw(monthStart.AddDays(2), "X");

        var result = await Repo().AggregateMonthlyAsync(now.Year, now.Month);

        result.Single().Timestamp.Should().Be(monthStart);
    }
}
