namespace OSFRS.Backend.DTOs;

public record AnomalyPointDto
{
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
    public string Reason { get; set; } = string.Empty;
}