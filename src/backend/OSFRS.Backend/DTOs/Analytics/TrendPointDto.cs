namespace OSFRS.Backend.DTOs.Analytics;

public record TrendPointDto
{
    public DateTime Timestamp { get; init; }
    public int Count { get; init; }
}