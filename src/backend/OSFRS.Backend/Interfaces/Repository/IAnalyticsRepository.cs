using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

/// <summary>
/// Provides read-only access to analytics-related data, including
/// trend points and raw usage events.
/// </summary>
/// <remarks>
/// This repository is used by the analytics and reporting services to
/// compute trends, detect anomalies, and generate analytical views
/// of system activity.
/// </remarks>
public interface IAnalyticsRepository
{
    /// <summary>
    /// Retrieves daily aggregated usage counts within the specified date range.
    /// </summary>
    /// <param name="from">The start of the date range (inclusive).</param>
    /// <param name="to">The end of the date range (inclusive).</param>
    /// <returns>
    /// A sequence of <see cref="TrendPointDto"/> entries representing
    /// daily usage counts.
    /// </returns>
    Task<IEnumerable<TrendPointDto>> GetDailyCountsAsync(DateTime from, DateTime to);

    /// <summary>
    /// Retrieves monthly aggregated usage counts for a specific year.
    /// </summary>
    /// <param name="year">The target year.</param>
    /// <returns>
    /// A sequence of <see cref="TrendPointDto"/> entries summarizing
    /// system activity per month.
    /// </returns>
    Task<IEnumerable<TrendPointDto>> GetMonthlyCountsAsync(int year);

    /// <summary>
    /// Retrieves raw <see cref="UsageRecord"/> entries within a
    /// specified date range. Used for anomaly detection, visualization,
    /// and low-level analytics computations.
    /// </summary>
    /// <param name="from">The earliest timestamp to include.</param>
    /// <param name="to">The latest timestamp to include.</param>
    /// <returns>A sequence of <see cref="UsageRecord"/> events.</returns>
    Task<IEnumerable<UsageRecord>> GetRawEventsAsync(DateTime from, DateTime to);
}