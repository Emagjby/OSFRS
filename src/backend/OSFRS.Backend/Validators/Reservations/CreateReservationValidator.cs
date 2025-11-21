using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;

namespace OSFRS.Backend.Validators.Reservations;

public class CreateReservationValidator : BaseValidator, IValidator<(CreateReservationDto dto, int userId)>
{
    private readonly IFacilityRepository _facility;
    private readonly IReservationRepository _reservation;
    private readonly IMaintenanceRepository _maintenance;

    public CreateReservationValidator(
        IFacilityRepository facility,
        IReservationRepository reservation,
        IMaintenanceRepository maintenance)
    {
        _facility = facility;
        _reservation = reservation;
        _maintenance = maintenance;
    }

    public async Task ValidateAsync((CreateReservationDto dto, int userId) input)
    {
        var (dto, userId) = input;

        EnsureValidId(dto.FacilityId, "Invalid facility ID.");

        var facility = await _facility.GetByIdAsync(dto.FacilityId);
        EnsureFound(facility, "Facility not found.");

        EnsureValidTimeRange(dto.StartTime, dto.EndTime, "StartTime must be before EndTime.");
        EnsureNotPast(dto.StartTime, "Cannot create a reservation in the past.");

        var maintenance = await _maintenance.GetByFacilityAsync(dto.FacilityId);
        bool overlapsMaintenance = maintenance.Any(m =>
            dto.StartTime < m.EndTime && dto.EndTime > m.StartTime);

        if (overlapsMaintenance)
            Forbidden("Facility is under maintenance during the selected time window.");

        bool available = await _reservation.IsSlotAvailableAsync(
            dto.StartTime, dto.EndTime, dto.FacilityId);

        EnsureNoConflict(available, "The selected time slot is unavailable.");

        Require(userId > 0, "Invalid user.");
    }
}