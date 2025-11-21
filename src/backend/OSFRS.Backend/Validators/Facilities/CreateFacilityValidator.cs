using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;

namespace OSFRS.Backend.Validators.Facilities;

public class CreateFacilityValidator : BaseValidator, IValidator<CreateFacilityDto>
{
    private static readonly string[] AllowedStatuses =
    {
        "Available", "Unavailable", "UnderMaintenance"
    };

    // private static readonly string[] AllowedTypes =
    // {
    //     "Court", "Gym", "Pool", "Hall" 
    // }; - future

    public Task ValidateAsync(CreateFacilityDto dto)
    {
        // NAME
        Require(!string.IsNullOrWhiteSpace(dto.Name), "Name is required.");

        // TYPE
        Require(!string.IsNullOrWhiteSpace(dto.Type), "Facility type is required.");
        // Require(AllowedTypes.Contains(dto.Type), $"Invalid facility type '{dto.Type}'."); - future

        // CAPACITY
        Require(dto.Capacity > 0, "Capacity must be greater than zero.");

        // STATUS
        Require(AllowedStatuses.Contains(dto.Status), $"Invalid status '{dto.Status}'.");

        return Task.CompletedTask;
    }
}