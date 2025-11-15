using Microsoft.AspNetCore.Mvc.ModelBinding;
using OSFRS.Backend.DTOs;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Validators;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class UsageService : IUsageService
{
    private readonly IUsageRepository _repo;
    private readonly IAppLogger<UsageService> _logger;

    public UsageService(IUsageRepository repo, IAppLogger<UsageService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task AggregateAsync()
    {
        var dailyAgg = await _repo.AggregateDailyAsync(date: DateTime.UtcNow);
        var monthlyAgg = await _repo.AggregateMonthlyAsync(
            year: DateTime.UtcNow.Year,
            month: DateTime.UtcNow.Month
        );

        await LogEventAsync(
            UsageEventBuilder.Create(
                UsageEventTypes.AggregateComputed
            )
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
                    AggregatedData = dto.Metadata != null
                        ? System.Text.Json.JsonSerializer.Serialize(dto.Metadata)
                        : null
                };
            }
        );

        await _repo.AddRangeAsync(usageRecords);

        _logger.LogInformation(
            "Bulk logged {Count} usage events.",
            usageRecords.Count()
        );
    }

    public Task<IEnumerable<UsageRecord>> GetDailyAggregateAsync(DateTime date)
    {
        return _repo.GetDailyAnalyticsAsync(date);
    }

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

    public Task<IEnumerable<UsageRecord>> GetMonthlyAggregateAsync(int year, int month)
    {
        return GetMonthlyAggregateAsync(year, month);
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
            AggregatedData = dto.Metadata != null
                ? System.Text.Json.JsonSerializer.Serialize(dto.Metadata)
                : null
        };

        await _repo.AddAsync(usageRecord);

        _logger.LogInformation(
            "Usage event logged: {EventType}, User {UserId}, Facility {FacilityId}",
            usageRecord.EventType,
            usageRecord.UserId!,
            usageRecord.FacilityId!
        );
    }

}