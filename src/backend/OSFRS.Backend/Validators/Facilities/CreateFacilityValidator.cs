using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Facilities;

/// <summary>
/// Validates input for creating a new <see cref="Facility"/>.
/// Ensures all required fields are provided and meet structural rules.
/// </summary>
public class CreateFacilityValidator : BaseValidator, IValidator<CreateFacilityDto>
{
    private static readonly string[] AllowedStatuses =
    {
        "Available",
        "Unavailable",
        "UnderMaintenance"
    };

    /// <summary>
    /// Validates the provided <see cref="CreateFacilityDto"/> instance.
    /// </summary>
    /// <param name="dto">The facility creation DTO to validate.</param>
    /// <returns>A completed task once validation passes.</returns>
    public Task ValidateAsync(CreateFacilityDto dto)
    {
        // NAME
        Require(!string.IsNullOrWhiteSpace(dto.Name), "Name is required.");
        Require(dto.Name.Length <= 100, "Name cannot exceed 100 characters.");

        // TYPE
        Require(!string.IsNullOrWhiteSpace(dto.Type), "Facility type is required.");
        Require(dto.Type.Length <= 50, "Type cannot exceed 50 characters.");
        // Future enhancement:
        // Require(AllowedTypes.Contains(dto.Type), $"Invalid facility type '{dto.Type}'.");

        // CAPACITY
        Require(dto.Capacity > 0, "Capacity must be greater than zero.");

        // STATUS
        Require(
            AllowedStatuses.Contains(dto.Status),
            $"Invalid status '{dto.Status}'."
        );

        return Task.CompletedTask;
    }
}