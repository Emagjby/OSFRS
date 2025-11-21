using OSFRS.Backend.DTOs.Reports;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Reservations;

public class UpdateReservationValidator :
    BaseValidator,
    IValidator<(UpdateReservationDto dto, Reservation existing, bool isAdmin, int userId)>
{
    private readonly IMaintenanceRepository _maintenance;
    private readonly IReservationRepository _reservation;

    public UpdateReservationValidator(
        IMaintenanceRepository maintenance,
        IReservationRepository reservation)
    {
        _maintenance = maintenance;
        _reservation = reservation;
    }

    public async Task ValidateAsync((UpdateReservationDto dto, Reservation existing, bool isAdmin, int userId) input)
    {
        var (dto, existing, isAdmin, userId) = input;

        EnsureFound(existing, "Reservation not found.");

        if (!isAdmin && existing.UserId != userId)
            Forbidden("You do not have permission to modify this reservation.");

        if (existing.Status == "Cancelled" && !isAdmin)
            Forbidden("Cancelled reservations cannot be modified.");

        if (existing.StartTime < DateTime.UtcNow && !isAdmin)
            Forbidden("Past reservations cannot be modified.");

        EnsureValidTimeRange(dto.StartTime, dto.EndTime, "StartTime must be before EndTime.");
        EnsureNotPast(dto.StartTime, "Reservation cannot start in the past.");

        var maintenance = await _maintenance.GetByFacilityAsync(existing.FacilityId);
        bool maintenanceConflict = maintenance.Any(m =>
            dto.StartTime < m.EndTime && dto.EndTime > m.StartTime);

        if (maintenanceConflict)
            Forbidden("Facility is under maintenance during the selected time window.");

        bool hasConflict = await _reservation.HasConflictAsync(
            existing.FacilityId,
            dto.StartTime,
            dto.EndTime,
            excludeReservationId: existing.Id);

        if (hasConflict && !isAdmin)
            Forbidden("The selected time slot is already taken.");
    }
}