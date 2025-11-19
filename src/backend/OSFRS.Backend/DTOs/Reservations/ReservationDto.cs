namespace OSFRS.Backend.DTOs.Reservations;

public record ReservationDto
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public int FacilityId { get; init; }

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public string Status { get; init; } = null!;

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}