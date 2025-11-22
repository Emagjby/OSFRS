using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

/// <summary>
/// Provides data-access operations for generating and persisting system usage reports.
/// </summary>
/// <remarks>
/// This repository is responsible for retrieving aggregated usage data and storing
/// completed report snapshots for later retrieval or export.
/// </remarks>
public interface IReportRepository
{
    /// <summary>
    /// Retrieves daily usage aggregates for the specified UTC date.
    /// </summary>
    /// <param name="dayUtc">The target day (UTC) for which aggregates should be returned.</param>
    /// <returns>
    /// A collection of <see cref="UsageRecord"/> representing daily aggregated data.
    /// </returns>
    Task<IEnumerable<UsageRecord>> GetDailyAggregatesAsync(DateTime dayUtc);

    /// <summary>
    /// Retrieves monthly usage aggregates for the specified year and month.
    /// </summary>
    /// <param name="year">The target year.</param>
    /// <param name="month">The target month (1–12).</param>
    /// <returns>
    /// A collection of <see cref="UsageRecord"/> representing monthly aggregated data.
    /// </returns>
    Task<IEnumerable<UsageRecord>> GetMonthlyAggregatesAsync(int year, int month);

    /// <summary>
    /// Persists a report entity to the database.
    /// </summary>
    /// <param name="report">The report entity to save.</param>
    /// <returns>
    /// The saved <see cref="Report"/> entity, including generated identifiers or timestamps.
    /// </returns>
    Task<Report> SaveReportAsync(Report report);
}