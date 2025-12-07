namespace OSFRS.Backend.DTOs.Reservations;

/// <summary>
/// Represents the data required to update an existing reservation.
/// </summary>
public record UpdateReservationDto
{
    /// <summary>
    /// Optional updated start time for the reservation (UTC).
    /// Must be earlier than <see cref="EndTime"/> when provided.
    /// </summary>
    public DateTime? StartTime { get; init; }

    /// <summary>
    /// Optional updated end time for the reservation (UTC).
    /// Must be later than <see cref="StartTime"/> when provided.
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// Optional updated status for the reservation.
    /// Admin-only field. Valid values include:
    /// Pending, Confirmed, Cancelled.
    /// </summary>
    public string? Status { get; init; }
}
