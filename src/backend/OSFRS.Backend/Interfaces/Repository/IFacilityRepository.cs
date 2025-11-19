using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

public interface IFacilityRepository : IBaseRepository<Facility>
{
    Task UpdateAvailabilityAsync(int facilityId, bool isAvailable);
    Task<bool> IsFacilityAvailableAsync(int facilityId);
}