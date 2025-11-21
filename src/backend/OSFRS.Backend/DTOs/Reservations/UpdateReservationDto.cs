namespace OSFRS.Backend.DTOs.Reservations;

public record UpdateReservationDto
{
    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public string? Status { get; init; }
}