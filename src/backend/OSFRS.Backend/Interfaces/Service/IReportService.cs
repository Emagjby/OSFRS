using OSFRS.Backend.DTOs.Reports;

namespace OSFRS.Backend.Interfaces.Service;

/// <summary>
/// Provides report generation and export functionality for usage statistics.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generates a daily usage report.
    /// </summary>
    /// <param name="dateTime">
    /// The day for which the report should be generated.  
    /// If null, the current day (UTC) is used.
    /// </param>
    /// <returns>
    /// A <see cref="ReportResultDto"/> containing daily usage data.
    /// </returns>
    Task<ReportResultDto> GetDailyReportAsync(DateTime? dateTime = null);

    /// <summary>
    /// Generates a monthly usage report.
    /// </summary>
    /// <param name="year">The target year. If null, the current UTC year is used.</param>
    /// <param name="month">The target month. If null, the current UTC month is used.</param>
    /// <returns>
    /// A <see cref="ReportResultDto"/> containing monthly usage data.
    /// </returns>
    Task<ReportResultDto> GetMonthlyReportAsync(int? year = null, int? month = null);

    /// <summary>
    /// Exports a daily usage report to a CSV file.
    /// </summary>
    /// <param name="date">
    /// The day for which the CSV should be generated.  
    /// If null, the current day (UTC) is used.
    /// </param>
    /// <returns>
    /// A byte array containing the CSV-encoded report.
    /// </returns>
    Task<byte[]> ExportCsvAsync(DateTime? date = null);

    /// <summary>
    /// Exports a daily usage report to a PDF file.
    /// </summary>
    /// <param name="date">
    /// The day for which the PDF should be generated.  
    /// If null, the current day (UTC) is used.
    /// </param>
    /// <returns>
    /// A byte array containing the PDF-encoded report content.
    /// </returns>
    Task<byte[]> ExportPdfAsync(DateTime? date = null);
}