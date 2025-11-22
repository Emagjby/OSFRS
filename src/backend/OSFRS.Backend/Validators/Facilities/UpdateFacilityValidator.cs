using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Facilities;

/// <summary>
/// Validates updates applied to an existing <see cref="Facility"/>.
/// Ensures name, capacity, and status changes follow system rules,
/// including restrictions caused by active maintenance.
/// </summary>
public class UpdateFacilityValidator : BaseValidator, IUpdateValidator<UpdateFacilityDto, Facility>
{
    private readonly IMaintenanceRepository _repo;

    private static readonly string[] AllowedStatuses =
    {
        "Available", "Unavailable", "UnderMaintenance"
    };

    // Future expansion:
    // private static readonly string[] AllowedTypes = { "Court", "Gym", "Pool", "Hall" };

    /// <summary>
    /// Creates a new instance of <see cref="UpdateFacilityValidator"/>.
    /// </summary>
    /// <param name="repo">Repository for querying maintenance activity associated with the facility.</param>
    public UpdateFacilityValidator(IMaintenanceRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Validates that the given update DTO can be safely applied to the existing facility.
    /// </summary>
    /// <param name="dto">The proposed update values.</param>
    /// <param name="existing">The current facility entity from the database.</param>
    /// <returns>A completed task if validation succeeds.</returns>
    public async Task ValidateAsync(UpdateFacilityDto dto, Facility existing)
    {
        // NAME
        if (dto.Name is not null)
        {
            Require(!string.IsNullOrWhiteSpace(dto.Name), "Name cannot be empty.");
        }

        // TYPE (future rules)
        // if (dto.Type is not null)
        //     Require(AllowedTypes.Contains(dto.Type), $"Invalid facility type '{dto.Type}'.");

        // CAPACITY
        if (dto.Capacity is not null)
        {
            Require(dto.Capacity > 0, "Capacity must be greater than zero.");
        }

        // STATUS
        if (dto.Status is not null)
        {
            Require(AllowedStatuses.Contains(dto.Status), $"Invalid status '{dto.Status}'.");

            // Prevent UnderMaintenance -> Available transition unless maintenance ended
            if (existing.Status == "UnderMaintenance" && dto.Status == "Available")
            {
                Forbidden("Facility cannot be marked as Available until maintenance ends.");
            }

            // Prevent marking Available during active maintenance
            var now = DateTime.UtcNow;
            var activeMaintenance = await _repo.GetByFacilityAsync(existing.Id);

            bool isUnderMaintenance = activeMaintenance.Any(m =>
                now >= m.StartTime && now <= m.EndTime
            );

            if (dto.Status == "Available" && isUnderMaintenance)
            {
                Forbidden("Facility is currently under maintenance and cannot be marked Available.");
            }
        }
    }
}