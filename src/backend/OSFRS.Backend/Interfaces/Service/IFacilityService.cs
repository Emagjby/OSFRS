using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Interfaces.Base;

namespace OSFRS.Backend.Interfaces.Service;

public interface IFacilityService : ICrudService<CreateFacilityDto, UpdateFacilityDto, FacilityDto>
{
    Task<bool> UpdateAvailabilityAsync(int facilityId, bool isAvailable);
    Task<bool> IsFacilityAvailableAsync(int facilityId);
}