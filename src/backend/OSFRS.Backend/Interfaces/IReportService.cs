using OSFRS.Backend.DTOs;

namespace OSFRS.Backend.Interfaces;

public interface IReportService
{
    Task<ReportResultDto> GetDailyReportAsync(DateTime? dateTime = null);
    Task<ReportResultDto> GetMonthlyReportAsync(int? year = null, int? month = null);

    Task<byte[]> ExportCsvAsync(DateTime? date = null);
    Task<byte[]> ExportPdfAsync(DateTime? date = null);
}