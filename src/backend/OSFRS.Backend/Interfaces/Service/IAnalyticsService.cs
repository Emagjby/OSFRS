using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Backend.DTOs.Reports;

namespace OSFRS.Backend.Interfaces.Service;

public interface IAnalyticsService
{
    Task<TrendReportDto> GetDailyTrendsAsync(DateTime from, DateTime to);
    Task<TrendReportDto> GetMonthlyTrendsAsync(int year);
    Task<AnomalyReportDto> DetectAnomaliesAsync(DateTime from, DateTime to, string mode = "z-score");
    Task<PeakUsageDto> GetPeakUsageAsync(DateTime from, DateTime to);
    Task<VisualizationDataDto> GetVisualizationDataAsync(DateTime from, DateTime to);
}