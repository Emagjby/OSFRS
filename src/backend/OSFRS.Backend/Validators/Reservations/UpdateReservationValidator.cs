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
            Forbidden("You do not have permission to modify this reservation.");

        // Cancelled reservations cannot be modified by non-admins
        if (existing.Status == "Cancelled" && !isAdmin)
            Forbidden("Cancelled reservations cannot be modified.");

        // Past reservations are locked for non-admins
        if (existing.StartTime < DateTime.UtcNow && !isAdmin)
            Forbidden("Past reservations cannot be modified.");

        // Time range validation
        EnsureValidTimeRange(dto.StartTime, dto.EndTime, "StartTime must be before EndTime.");
        EnsureNotPast(dto.StartTime, "Reservation cannot start in the past.");

        // Maintenance overlap validation
        var maintenance = await _maintenance.GetByFacilityAsync(existing.FacilityId);
        bool maintenanceConflict = maintenance.Any(m =>
            dto.StartTime < m.EndTime &&
            dto.EndTime > m.StartTime
        );

        if (maintenanceConflict)
            Forbidden("Facility is under maintenance during the selected time window.");

        // Reservation conflict check
        bool hasConflict = await _reservation.HasConflictAsync(
            existing.FacilityId,
            dto.StartTime,
            dto.EndTime,
            excludeReservationId: existing.Id
        );

        if (hasConflict && !isAdmin)
            Forbidden("The selected time slot is already taken.");
    }
}