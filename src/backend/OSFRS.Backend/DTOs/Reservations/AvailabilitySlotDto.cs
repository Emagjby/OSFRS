namespace OSFRS.Backend.DTOs.Reservations;

public record AvailabilitySlotDto
{
    public int Id { get; init; }

    public int FacilityId { get; init; }

    public int UserId { get; init; }

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public string Status { get; init; } = null!;
}