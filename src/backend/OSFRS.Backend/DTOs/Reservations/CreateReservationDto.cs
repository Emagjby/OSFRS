namespace OSFRS.Backend.DTOs.Reservations;

public record CreateReservationDto
{
    public int FacilityId { get; init; }

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }
}