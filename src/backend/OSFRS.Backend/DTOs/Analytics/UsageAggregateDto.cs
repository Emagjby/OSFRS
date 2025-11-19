namespace OSFRS.Backend.DTOs.Analytics;

public record UsageAggregateDto
{
    public string EventType { get; init; } = null!;

    public int Count { get; init; }

    public DateTime PeriodStart { get; init; }

    public DateTime PeriodEnd { get; init; }

    public int? UserId { get; init; }
    public int? FacilityId { get; init; }
}