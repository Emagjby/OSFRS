using FluentAssertions;
using Moq;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Services;
using OSFRS.Models.Entities;
using OSFRS.UnitTests.TestUtils;

namespace OSFRS.UnitTests.Services;

public class UsageServiceTests
{
    private readonly Mock<IUsageRepository> _repo;
    private readonly Mock<IAppLogger<UsageService>> _logger;
    private readonly Mock<
        IValidator<(string? eventType, int? userId, int? facilityId, DateTime? from, DateTime? to)>
    > _validator;

    private readonly UsageService _service;

    public UsageServiceTests()
    {
        _repo = MockFactories.UsageRepo();
        _logger = MockFactories.Logger<UsageService>();
        _validator = MockFactories.Validator<(string?, int?, int?, DateTime?, DateTime?)>();

        _service = new UsageService(_repo.Object, _logger.Object, _validator.Object);
    }

    // ============================================================
    // LOG SINGLE EVENT
    // ============================================================

    [Fact]
    public async Task LogEvent_ShouldAddRecord_AndSave()
    {
        var dto = FakeData.UsageEvent().Generate();

        await _service.LogEventAsync(dto);

        _repo.Verify(
            r => r.AddAsync(It.IsAny<UsageRecord>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogEvent_ShouldMapDtoCorrectly()
    {
        var dto = FakeData.UsageEvent().Generate();
        UsageRecord? captured = null;

        _repo
            .Setup(r => r.AddAsync(It.IsAny<UsageRecord>(), It.IsAny<CancellationToken>()))
            .Callback<UsageRecord, CancellationToken>((rec, _) => captured = rec);

        await _service.LogEventAsync(dto);

        captured.Should().NotBeNull();
        captured!.EventType.Should().Be(dto.EventType);
        captured.UserId.Should().Be(dto.UserId);
        captured.FacilityId.Should().Be(dto.FacilityId);
        captured.Timestamp.Should().Be(dto.Timestamp);
    }

    // ============================================================
    // BULK LOG
    // ============================================================

    [Fact]
    public async Task BulkLog_ShouldAddRange_AndSave()
    {
        var list = FakeData.UsageEvent().Generate(3);

        await _service.BulkLogAsync(list);

        _repo.Verify(
            r => r.AddRangeAsync(It.IsAny<List<UsageRecord>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BulkLog_ShouldMapAllEventsCorrectly()
    {
        var dtos = FakeData.UsageEvent().Generate(2);
        List<UsageRecord>? captured = null;

        _repo
            .Setup(r =>
                r.AddRangeAsync(It.IsAny<IEnumerable<UsageRecord>>(), It.IsAny<CancellationToken>())
            )
            .Callback<IEnumerable<UsageRecord>, CancellationToken>(
                (recs, _) => captured = recs.ToList()
            );

        await _service.BulkLogAsync(dtos);

        captured.Should().NotBeNull();
        captured!.Count.Should().Be(2);

        for (int i = 0; i < 2; i++)
        {
            captured[i].EventType.Should().Be(dtos[i].EventType);
            captured[i].Timestamp.Should().Be(dtos[i].Timestamp);
        }
    }

    // ============================================================
    // AGGREGATION
    // ============================================================

    [Fact]
    public async Task Aggregate_ShouldCallDaily_AndMonthly_AndSave()
    {
        var now = DateTime.UtcNow;

        _repo
            .Setup(r => r.AggregateDailyAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<UsageRecord>());

        _repo
            .Setup(r => r.AggregateMonthlyAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<UsageRecord>());

        await _service.AggregateAsync();

        _repo.Verify(r => r.AggregateDailyAsync(It.IsAny<DateTime>()), Times.Once);
        _repo.Verify(r => r.AggregateMonthlyAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

        // SaveChanges called:
        // 1) after aggregations
        // 2) inside LogEventAsync
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // ============================================================
    // DAILY / MONTHLY READS
    // ============================================================

    [Fact]
    public async Task GetDailyAggregate_ShouldReturnRepoResults()
    {
        var date = DateTime.UtcNow.Date;
        var list = FakeData.UsageRecord().Generate(2);

        _repo.Setup(r => r.GetDailyAnalyticsAsync(date)).ReturnsAsync(list);

        var result = await _service.GetDailyAggregateAsync(date);

        result.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetMONTHlyAggregate_ShouldReturnRepoResults()
    {
        int YEAR = 2025;
        int MONTH = 1;

        var list = FakeData.UsageRecord().Generate(3);

        _repo.Setup(r => r.GetMonthlyAnalyticsAsync(YEAR, MONTH)).ReturnsAsync(list);

        var result = await _service.GetMonthlyAggregateAsync(YEAR, MONTH);

        result.Should().BeEquivalentTo(list);
    }

    // ============================================================
    // QUERY WITH VALIDATOR
    // ============================================================

    [Fact]
    public async Task GetEvents_ShouldCallValidator_AndReturnResults()
    {
        var EVENT = "Created";
        var USER = 5;

        var list = FakeData.UsageRecord().Generate(2);

        _repo.Setup(r => r.QueryAsync(EVENT, USER, null, null, null)).ReturnsAsync(list);

        var result = await _service.GetEventsAsync(eventType: EVENT, userId: USER);

        _validator.Verify(
            v =>
                v.ValidateAsync(
                    It.Is<(string? eventType, int? userId, int?, DateTime?, DateTime?)>(t =>
                        t.eventType == EVENT && t.userId == USER
                    )
                ),
            Times.Once
        );

        result.Should().BeEquivalentTo(list);
    }
}
