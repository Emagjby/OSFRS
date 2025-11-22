namespace OSFRS.Backend.DTOs.Reservations;

/// <summary>
/// Represents a reservation made by a user for a specific facility,
/// including its lifecycle metadata.
/// </summary>
public record ReservationDto
{
    /// <summary>
    /// Unique identifier of the reservation.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Identifier of the user who created the reservation.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Identifier of the facility being reserved.
    /// </summary>
    public int FacilityId { get; init; }

    /// <summary>
    /// Start time of the reservation (UTC).
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// End time of the reservation (UTC).
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Current status of the reservation.
    /// Possible values include:
    /// Pending, Confirmed, Cancelled.
    /// </summary>
    public string Status { get; init; } = null!;

    /// <summary>
    /// Timestamp (UTC) indicating when the reservation was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp (UTC) indicating the last update to the reservation.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}