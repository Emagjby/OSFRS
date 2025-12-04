using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Reservations;

/// <summary>
/// Validates cancellation requests for an existing <see cref="Reservation"/>.
/// Ensures the reservation exists, is owned by the requesting user, and is not already cancelled.
/// </summary>
public class CancelReservationValidator : BaseValidator, IValidator<(Reservation reservation, int userId)>
{
    /// <summary>
    /// Validates whether a reservation can be cancelled by the specified user.
    /// </summary>
    /// <param name="data">Tuple containing the reservation instance and 
    /// the ID of the user attempting the cancellation.</param>
    /// <returns>A completed task if validation succeeds.</returns>
    public Task ValidateAsync((Reservation reservation, int userId) data)
    {
        var (reservation, userId) = data;

        EnsureFound(reservation, "Reservation not found.");

        Require(reservation.UserId == userId, "You do not own this reservation.");

        EnsureNotPast(reservation.StartTime, "Cannot cancel a reservation in the past.");

        if (reservation.Status == "Cancelled")
            Conflict("Reservation is already cancelled.");

        return Task.CompletedTask;
    }
}