using OSFRS.Backend.DTOs;

namespace OSFRS.Backend.Interfaces;

public interface IFacilityService : ICrudService<CreateFacilityDto, UpdateFacilityDto, FacilityDto>
{
    Task<bool> UpdateAvailabilityAsync(int facilityId, bool isAvailable);
    Task<bool> IsFacilityAvailableAsync(int facilityId);
}