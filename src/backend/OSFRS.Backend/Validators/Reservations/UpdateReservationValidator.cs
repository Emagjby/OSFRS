using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Reservations;

/// <summary>
/// Validates update requests for existing reservations.
/// Ensures ownership rules, time window validity, maintenance conflict rules,
/// and global business logic constraints are respected.
/// </summary>
public class UpdateReservationValidator :
    BaseValidator,
    IValidator<(UpdateReservationDto dto, Reservation existing, bool isAdmin, int userId)>
{
    private readonly IMaintenanceRepository _maintenance;
    private readonly IReservationRepository _reservation;

    /// <summary>
    /// Creates a new validator for reservation updates.
    /// </summary>
    public UpdateReservationValidator(
        IMaintenanceRepository maintenance,
        IReservationRepository reservation)
    {
        _maintenance = maintenance;
        _reservation = reservation;
    }

    /// <summary>
    /// Validates that the reservation update request meets all required rules.
    /// </summary>
    /// <param name="input">
    /// Tuple containing the update DTO, the existing reservation,
    /// a flag indicating whether the caller is an admin,
    /// and the ID of the requesting user.
    /// </param>
    public async Task ValidateAsync((UpdateReservationDto dto, Reservation existing, bool isAdmin, int userId) input)
    {
        var (dto, existing, isAdmin, userId) = input;

        EnsureFound(existing, "Reservation not found.");

        // Ownership check (non-admins can only modify their own reservations)
        if (!isAdmin && existing.UserId != userId)
            Conflict("You do not have permission to modify this reservation.");

        // Cancelled reservations cannot be modified by non-admins
        if (existing.Status == "Cancelled" && !isAdmin)
            Conflict("Cancelled reservations cannot be modified.");

        // Past reservations are locked for non-admins
        if (existing.StartTime < DateTime.UtcNow && !isAdmin)
            Conflict("Past reservations cannot be modified.");

        // Status rules
        if (!isAdmin && dto.Status is not null)
            Conflict("You are not allowed to modify the reservation status.");

        if (dto.Status is not null)
        {
            var allowedStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
            Require(allowedStatuses.Contains(dto.Status), "Invalid reservation status.");
        }

        // Time payload requirements
        if (dto.StartTime.HasValue != dto.EndTime.HasValue)
            Conflict("StartTime and EndTime must both be provided when updating the schedule.");

        if (!isAdmin)
        {
            Require(dto.StartTime.HasValue && dto.EndTime.HasValue, "StartTime and EndTime are required.");
        }

        var targetStart = dto.StartTime ?? existing.StartTime;
        var targetEnd = dto.EndTime ?? existing.EndTime;

        var shouldValidateTimeWindow = dto.StartTime.HasValue || dto.EndTime.HasValue || !isAdmin;

        if (shouldValidateTimeWindow)
        {
            EnsureValidTimeRange(targetStart, targetEnd, "StartTime must be before EndTime.");

            if (!isAdmin)
                EnsureNotPast(targetStart, "Reservation cannot start in the past.");

            var maintenance = await _maintenance.GetByFacilityAsync(existing.FacilityId);
            bool maintenanceConflict = maintenance.Any(m =>
                targetStart < m.EndTime &&
                targetEnd > m.StartTime
            );

            if (maintenanceConflict)
                Conflict("Facility is under maintenance during the selected time window.");

            bool hasConflict = await _reservation.HasConflictAsync(
                existing.FacilityId,
                targetStart,
                targetEnd,
                excludeReservationId: existing.Id
            );

            if (hasConflict && !isAdmin)
                Conflict("The selected time slot is already taken.");
        }
    }
}
