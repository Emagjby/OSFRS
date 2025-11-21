using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Facilities;

public class UpdateFacilityValidator : BaseValidator, IUpdateValidator<UpdateFacilityDto, Facility>
{
    private readonly IMaintenanceRepository _repo;

    private static readonly string[] AllowedStatuses =
    {
        "Available", "Unavailable", "UnderMaintenance"
    };

    // private static readonly string[] AllowedTypes =
    // {
    //     "Court", "Gym", "Pool", "Hall" // example – adjust
    // }; - future

    public UpdateFacilityValidator(IMaintenanceRepository repo)
    {
        _repo = repo;
    }

    public async Task ValidateAsync(UpdateFacilityDto dto, Facility existing)
    {
        // NAME
        if (dto.Name is not null)
            Require(!string.IsNullOrWhiteSpace(dto.Name), "Name cannot be empty.");

        // TYPE - future
        // if (dto.Type is not null)
        //     Require(AllowedTypes.Contains(dto.Type), $"Invalid facility type '{dto.Type}'.");

        // CAPACITY
        if (dto.Capacity is not null)
            Require(dto.Capacity > 0, "Capacity must be greater than zero.");

        // STATUS RULES
        if (dto.Status is not null)
        {
            Require(AllowedStatuses.Contains(dto.Status), $"Invalid status '{dto.Status}'.");

            // Cant manually switch UnderMaintenance -> Available
            if (existing.Status == "UnderMaintenance" && dto.Status == "Available")
                Forbidden("Facility cannot be marked as Available until maintenance ends.");

            // Cant mark facility Available if there is active maintenance
            var now = DateTime.UtcNow;
            var activeMaintenance = await _repo.GetByFacilityAsync(existing.Id);

            if (dto.Status == "Available" &&
                activeMaintenance.Any(m => now >= m.StartTime && now <= m.EndTime))
            {
                Forbidden("Facility is currently under maintenance and cannot be marked Available.");
            }
        }
    }
}