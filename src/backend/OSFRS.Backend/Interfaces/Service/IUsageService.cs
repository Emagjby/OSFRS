using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Service;

/// <summary>
/// Provides functionality for logging usage events, querying historic usage data,
/// and generating aggregate analytics for the OSFRS system.
/// </summary>
public interface IUsageService
{
    /// <summary>
    /// Logs a single usage event into the system.
    /// </summary>
    /// <param name="dto">The event payload to record.</param>
    Task LogEventAsync(UsageEventDto dto);

    /// <summary>
    /// Queries usage events with optional filtering parameters.
    /// </summary>
    /// <param name="eventType">Optional event type filter.</param>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="facilityId">Optional facility ID filter.</param>
    /// <param name="start">Optional start of the date range (UTC).</param>
    /// <param name="end">Optional end of the date range (UTC).</param>
    /// <returns>
    /// A collection of <see cref="UsageRecord"/> entries matching the given criteria.
    /// </returns>
    Task<IEnumerable<UsageRecord>> GetEventsAsync(
        string? eventType = null,
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    );

    /// <summary>
    /// Logs a batch of usage events in a single operation.
    /// </summary>
    /// <param name="dtos">The collection of event DTOs to record.</param>
    Task BulkLogAsync(IEnumerable<UsageEventDto> dtos);

    /// <summary>
    /// Executes system-wide aggregation logic for daily and monthly analytics.
    /// </summary>
    Task AggregateAsync();

    /// <summary>
    /// Retrieves daily aggregated usage statistics for the specified date.
    /// </summary>
    /// <param name="date">The UTC date for which aggregation results are requested.</param>
    /// <returns>
    /// A collection of aggregated <see cref="UsageRecord"/> entries.
    /// </returns>
    Task<IEnumerable<UsageRecord>> GetDailyAggregateAsync(DateTime date);

    /// <summary>
    /// Retrieves monthly aggregated usage statistics for the specified year and month.
    /// </summary>
    /// <param name="year">The target year.</param>
    /// <param name="month">The target month.</param>
    /// <returns>
    /// A collection of aggregated <see cref="UsageRecord"/> entries.
    /// </returns>
    Task<IEnumerable<UsageRecord>> GetMonthlyAggregateAsync(int year, int month);
}