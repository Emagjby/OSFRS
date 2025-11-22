using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

/// <summary>
/// Provides data-access operations for usage event records, including
/// querying, aggregation, and analytics support.
/// </summary>
public interface IUsageRepository : IBaseRepository<UsageRecord>
{
    /// <summary>
    /// Queries usage records with optional filters.
    /// </summary>
    /// <param name="eventType">Filter by event type (optional).</param>
    /// <param name="userId">Filter by user ID (optional).</param>
    /// <param name="facilityId">Filter by facility ID (optional).</param>
    /// <param name="start">Filter for events occurring after this timestamp (optional).</param>
    /// <param name="end">Filter for events occurring before this timestamp (optional).</param>
    /// <returns>
    /// A filtered collection of <see cref="UsageRecord"/> entities.
    /// </returns>
    Task<IEnumerable<UsageRecord>> QueryAsync(
        string? eventType = null,
        int? userId = null,
        int? facilityId = null,
        DateTime? start = null,
        DateTime? end = null
    );

    /// <summary>
    /// Computes aggregated daily metrics for a specific date.
    /// </summary>
    /// <param name="date">The date (UTC) for which to compute aggregates.</param>
    /// <returns>
    /// A collection of aggregated <see cref="UsageRecord"/> entries.
    /// </returns>
    Task<IEnumerable<UsageRecord>> AggregateDailyAsync(DateTime date);

    /// <summary>
    /// Computes aggregated monthly metrics.
    /// </summary>
    /// <param name="year">The target year.</param>
    /// <param name="month">The target month.</param>
    /// <returns>
    /// A collection of aggregated <see cref="UsageRecord"/> entries.
    /// </returns>
    Task<IEnumerable<UsageRecord>> AggregateMonthlyAsync(int year, int month);

    /// <summary>
    /// Retrieves daily analytics data for a specific date.
    /// </summary>
    /// <param name="date">The date (UTC) for which analytics should be fetched.</param>
    /// <returns>
    /// A collection of <see cref="UsageRecord"/> representing daily analytics.
    /// </returns>
    Task<IEnumerable<UsageRecord>> GetDailyAnalyticsAsync(DateTime date);

    /// <summary>
    /// Retrieves monthly analytics data for a specific month.
    /// </summary>
    /// <param name="year">The target year.</param>
    /// <param name="month">The target month.</param>
    /// <returns>
    /// A collection of <see cref="UsageRecord"/> representing monthly analytics.
    /// </returns>
    Task<IEnumerable<UsageRecord>> GetMonthlyAnalyticsAsync(int year, int month);
}