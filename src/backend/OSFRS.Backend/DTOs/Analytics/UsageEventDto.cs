namespace OSFRS.Backend.DTOs.Analytics;

public record UsageEventDto
{
    public string EventType { get; init; } = null!;

    public int? UserId { get; init; }
    public int? FacilityId { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public Dictionary<string, string>? Metadata { get; init; }
}