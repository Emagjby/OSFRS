using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Validators;
using OSFRS.Models.Entities;
using System.Text.Json;

namespace OSFRS.Backend.Services;

public class UsageService : IUsageService
{
    private readonly IUsageRepository _repo;
    private readonly IAppLogger<UsageService> _logger;

    public UsageService(
        IUsageRepository repo,
        IAppLogger<UsageService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task AggregateAsync()
    {
        var now = DateTime.UtcNow;

        var dailyAgg = await _repo.AggregateDailyAsync(now);
        var monthlyAgg = await _repo.AggregateMonthlyAsync(now.Year, now.Month);

        await _repo.SaveChangesAsync();

        await LogEventAsync(
            UsageEventBuilder.Create(UsageEventTypes.AggregateComputed)
        );

        _logger.LogInformation(
            "Aggregation complete. Daily: {DailyCount}, Monthly: {MonthlyCount}",
            dailyAgg.Count(),
            monthlyAgg.Count()
        );
    }

    public async Task BulkLogAsync(IEnumerable<UsageEventDto> dtos)
    {
        var usageRecords = dtos.Select(dto =>
        {
            if (!UsageEventValidator.Validate(dto))
                throw new ArgumentException("Invalid dto.");

            return new UsageRecord
            {
                EventType = dto.EventType,
                UserId = dto.UserId,
                FacilityId = dto.FacilityId,
                Timestamp = dto.Timestamp,
                AggregatedData = dto.Metadata is not null
                    ? JsonSerializer.Serialize(dto.Metadata)
                    : null
            };
        }).ToList();

        await _repo.AddRangeAsync(usageRecords);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Bulk logged {Count} usage events.",
            usageRecords.Count
        );
    }

    public async Task<IEnumerable<UsageRecord>> GetDailyAggregateAsync(DateTime date)
        => await _repo.GetDailyAnalyticsAsync(date);

    public async Task<IEnumerable<UsageRecord>> GetMonthlyAggregateAsync(int year, int month)
        => await _repo.GetMonthlyAnalyticsAsync(year, month);

    public async Task<IEnumerable<UsageRecord>> GetEventsAsync(
        string? eventType = null,
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null)
    {
        var events = await _repo.QueryAsync(eventType, userId, facilityId, start, end);

        _logger.LogInformation(
            "Fetched {Count} usage events.",
            events.Count()
        );

        return events;
    }

    public async Task LogEventAsync(UsageEventDto dto)
    {
        if (!UsageEventValidator.Validate(dto))
            throw new ArgumentException("Invalid dto.");

        var usageRecord = new UsageRecord
        {
            EventType = dto.EventType,
            UserId = dto.UserId,
            FacilityId = dto.FacilityId,
            Timestamp = dto.Timestamp,
            AggregatedData = dto.Metadata is not null
                ? JsonSerializer.Serialize(dto.Metadata)
                : null
        };

        await _repo.AddAsync(usageRecord);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Usage event logged: {EventType}, User {UserId}, Facility {FacilityId}",
            usageRecord.EventType,
            usageRecord.UserId!,
            usageRecord.FacilityId!
        );
    }
}