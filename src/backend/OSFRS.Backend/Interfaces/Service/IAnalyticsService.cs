using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Backend.DTOs.Reports;

namespace OSFRS.Backend.Interfaces.Service;

/// <summary>
/// Provides analytical operations for usage data, including trends,
/// anomaly detection, peak usage computation, and data visualization.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Computes daily usage trends for a specified date range.
    /// </summary>
    /// <param name="from">The start of the analysis window (UTC).</param>
    /// <param name="to">The end of the analysis window (UTC).</param>
    /// <returns>
    /// A <see cref="TrendReportDto"/> containing daily trend points,
    /// summary metrics, and optional moving averages.
    /// </returns>
    Task<TrendReportDto> GetDailyTrendsAsync(DateTime from, DateTime to);

    /// <summary>
    /// Computes monthly usage trends for a given year.
    /// </summary>
    /// <param name="year">The target year.</param>
    /// <returns>
    /// A <see cref="TrendReportDto"/> representing month-over-month analytics.
    /// </returns>
    Task<TrendReportDto> GetMonthlyTrendsAsync(int year);

    /// <summary>
    /// Detects anomalies in event frequencies within a specified time window.
    /// </summary>
    /// <param name="from">The start of the analysis window (UTC).</param>
    /// <param name="to">The end of the analysis window (UTC).</param>
    /// <param name="mode">
    /// The detection algorithm to use. Supported: <c>"z-score"</c> or <c>"mad"</c>.
    /// Defaults to <c>"z-score"</c>.
    /// </param>
    /// <returns>
    /// An <see cref="AnomalyReportDto"/> containing anomalous points and metadata.
    /// </returns>
    Task<AnomalyReportDto> DetectAnomaliesAsync(DateTime from, DateTime to, string mode = "z-score");

    /// <summary>
    /// Computes the highest usage peak in a given date range.
    /// </summary>
    /// <param name="from">The start of the analysis window (UTC).</param>
    /// <param name="to">The end of the analysis window (UTC).</param>
    /// <returns>
    /// A <see cref="PeakUsageDto"/> describing the maximum recorded peak.
    /// </returns>
    Task<PeakUsageDto> GetPeakUsageAsync(DateTime from, DateTime to);

    /// <summary>
    /// Generates chart-ready visualization data for analytical dashboards.
    /// </summary>
    /// <param name="from">The start of the visualization range (UTC).</param>
    /// <param name="to">The end of the visualization range (UTC).</param>
    /// <returns>
    /// A <see cref="VisualizationDataDto"/> containing labels, values,
    /// and metadata for graphical representation.
    /// </returns>
    Task<VisualizationDataDto> GetVisualizationDataAsync(DateTime from, DateTime to);
}