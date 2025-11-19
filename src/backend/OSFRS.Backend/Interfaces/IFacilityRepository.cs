using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IFacilityRepository : IBaseRepository<Facility>
{
    Task UpdateAvailabilityAsync(int facilityId, bool isAvailable);
    Task<bool> IsFacilityAvailableAsync(int facilityId);
}