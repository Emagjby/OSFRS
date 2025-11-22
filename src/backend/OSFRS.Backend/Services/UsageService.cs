using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Validators.Usage;
using OSFRS.Models.Entities;
using System.Text.Json;

namespace OSFRS.Backend.Services;

/// <summary>
/// Provides event logging, aggregation, and analytics-related operations
/// for usage tracking across the system.
/// </summary>
public class UsageService : IUsageService
{
    private readonly IUsageRepository _repo;
    private readonly IAppLogger<UsageService> _logger;
    private readonly UsageQueryValidator _validator;

    /// <summary>
    /// Initializes a new <see cref="UsageService"/> instance.
    /// </summary>
    /// <param name="repo">Repository handling persistence and analytics operations.</param>
    /// <param name="logger">Logger for diagnostics and auditing.</param>
    /// <param name="validator">Validator that ensures query parameters are valid.</param>
    public UsageService(
        IUsageRepository repo,
        IAppLogger<UsageService> logger,
        UsageQueryValidator validator)
    {
        _repo = repo;
        _logger = logger;
        _validator = validator;
    }

    /// <summary>
    /// Triggers daily and monthly aggregations and logs an aggregation event.
    /// </summary>
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

    /// <summary>
    /// Logs multiple usage events in bulk.
    /// </summary>
    /// <param name="dtos">The usage events to insert.</param>
    public async Task BulkLogAsync(IEnumerable<UsageEventDto> dtos)
    {
        var usageRecords = dtos.Select(dto => new UsageRecord
        {
            EventType = dto.EventType,
            UserId = dto.UserId,
            FacilityId = dto.FacilityId,
            Timestamp = dto.Timestamp,
            AggregatedData = dto.Metadata is not null
                ? JsonSerializer.Serialize(dto.Metadata)
                : null
        }).ToList();

        await _repo.AddRangeAsync(usageRecords);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Bulk logged {Count} usage events.",
            usageRecords.Count
        );
    }

    /// <summary>
    /// Retrieves all usage events that occurred on the specified day.
    /// </summary>
    public async Task<IEnumerable<UsageRecord>> GetDailyAggregateAsync(DateTime date)
        => await _repo.GetDailyAnalyticsAsync(date);

    /// <summary>
    /// Retrieves all usage events aggregated for a specific month.
    /// </summary>
    public async Task<IEnumerable<UsageRecord>> GetMonthlyAggregateAsync(int year, int month)
        => await _repo.GetMonthlyAnalyticsAsync(year, month);

    /// <summary>
    /// Queries usage events based on provided filters.
    /// </summary>
    /// <param name="eventType">Optional event type to filter.</param>
    /// <param name="userId">Optional user ID to filter.</param>
    /// <param name="facilityId">Optional facility ID to filter.</param>
    /// <param name="start">Optional start date/time.</param>
    /// <param name="end">Optional end date/time.</param>
    /// <returns>A filtered set of usage records.</returns>
    public async Task<IEnumerable<UsageRecord>> GetEventsAsync(
        string? eventType = null,
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null)
    {
        _validator.Validate(eventType, userId, facilityId, start, end);

        var events = await _repo.QueryAsync(eventType, userId, facilityId, start, end);

        _logger.LogInformation(
            "Fetched {Count} usage events.",
            events.Count()
        );

        return events;
    }

    /// <summary>
    /// Logs a single usage event.
    /// </summary>
    /// <param name="dto">Event data describing the operation performed.</param>
    public async Task LogEventAsync(UsageEventDto dto)
    {
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