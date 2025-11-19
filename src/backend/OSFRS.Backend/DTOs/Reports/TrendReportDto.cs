namespace OSFRS.Backend.DTOs;

public record TrendReportDto
{
    public required string RangeLabel { get; set; }
    public IEnumerable<TrendPointDto> Points { get; set; } = [];
    public int TotalCount { get; set; }
    public double AveragePerPoint { get; set; }
    public IEnumerable<double>? MovingAverage { get; set; }  
    public IEnumerable<double>? PercentageChange { get; set; } 
}