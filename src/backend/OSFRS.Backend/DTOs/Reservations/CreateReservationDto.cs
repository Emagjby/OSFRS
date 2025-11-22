namespace OSFRS.Backend.DTOs.Reservations;

/// <summary>
/// Represents the input required to create a new reservation
/// for a specific facility.
/// </summary>
public record CreateReservationDto
{
    /// <summary>
    /// Identifier of the facility being reserved.
    /// Must reference an existing facility.
    /// </summary>
    public int FacilityId { get; init; }

    /// <summary>
    /// Start time of the reservation (UTC).
    /// Must not be in the past and must be earlier than <see cref="EndTime"/>.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// End time of the reservation (UTC).
    /// Must be later than <see cref="StartTime"/>.
    /// </summary>
    public DateTime EndTime { get; init; }
}