namespace OSFRS.Backend.DTOs.Reservations;

/// <summary>
/// Represents the data required to update an existing reservation.
/// </summary>
public record UpdateReservationDto
{
    /// <summary>
    /// Updated start time for the reservation (UTC).
    /// Must be earlier than <see cref="EndTime"/>.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Updated end time for the reservation (UTC).
    /// Must be later than <see cref="StartTime"/>.
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Optional updated status for the reservation.
    /// Admin-only field. Valid values include:
    /// Pending, Confirmed, Cancelled.
    /// </summary>
    public string? Status { get; init; }
}