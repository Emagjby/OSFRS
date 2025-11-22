namespace OSFRS.Backend.DTOs.Reservations;

/// <summary>
/// Represents a single reservation slot within a facility's availability calendar.
/// </summary>
public record AvailabilitySlotDto
{
    /// <summary>
    /// Unique identifier of the reservation slot.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Identifier of the facility to which the slot belongs.
    /// </summary>
    public int FacilityId { get; init; }

    /// <summary>
    /// Identifier of the user who created the reservation.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Start time of the reservation (UTC).
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// End time of the reservation (UTC).
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Current status of the reservation (e.g., Pending, Confirmed, Cancelled).
    /// </summary>
    public string Status { get; init; } = null!;
}