using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators;

public static class FacilityValidator
{
    public static bool ValidateCreate(CreateFacilityDto dto, out string error)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            error = "Facility name cannot be empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Type))
        {
            error = "Facility type cannot be empty.";
            return false;
        }

        if (dto.Capacity < 1)
        {
            error = "Facility capacity must be greater than zero.";
            return false;
        }

        if (dto.Capacity > 1000)
        {
            error = "Facility capacity must be less than a thousand.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            error = "Facility status cannot be empty.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool ValidateUpdate(UpdateFacilityDto dto, Facility existing, out string error)
    {
        if (dto == null)
        {
            error = "Update data cannot be null.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Name) && string.IsNullOrWhiteSpace(dto.Type)
           && string.IsNullOrWhiteSpace(dto.Status) && dto.Capacity == null)
        {
            error = "No updates provided.";
            return false;
        }

        if (dto.Capacity != null && dto.Capacity < 1)
        {
            error = "Facility capacity must be greater than zero.";
            return false;
        }

        if (dto.Capacity != null && dto.Capacity > 1000)
        {
            error = "Facility capacity must be less than a thousand.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}