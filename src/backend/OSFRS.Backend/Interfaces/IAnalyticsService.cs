using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IAnalyticsService
{
    Task<TrendReportDto> GetDailyTrendsAsync(DateTime from, DateTime to);
    Task<TrendReportDto> GetMonthlyTrendsAsync(int year);
    Task<AnomalyReportDto> DetectAnomaliesAsync(DateTime from, DateTime to, string mode = "z-score");
    Task<PeakUsageDto> GetPeakUsageAsync(DateTime from, DateTime to);
    Task<VisualizationDataDto> GetVisualizationDataAsync(DateTime from, DateTime to);
}