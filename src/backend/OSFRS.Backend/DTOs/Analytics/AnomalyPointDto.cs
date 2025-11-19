namespace OSFRS.Backend.DTOs.Analytics;

public record AnomalyPointDto
{
    public DateTime Timestamp { get; init; }
    public int Count { get; init; }
    public string Reason { get; init; } = string.Empty;
}