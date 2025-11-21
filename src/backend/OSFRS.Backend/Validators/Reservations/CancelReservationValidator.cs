using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Reservations;

public class CancelReservationValidator : BaseValidator
{
    public Task ValidateAsync(Reservation reservation, int userId)
    {
        EnsureFound(reservation, "Reservation not found.");
        Require(reservation.UserId == userId, "You do not own this reservation.");

        if (reservation.Status == "Cancelled")
            Forbidden("Reservation is already cancelled.");

        return Task.CompletedTask;
    }
}