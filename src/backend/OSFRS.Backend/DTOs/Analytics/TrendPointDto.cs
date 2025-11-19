namespace OSFRS.Backend.DTOs;

public record TrendPointDto
{
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
}