using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IFacilityService
{
    Task<IEnumerable<Facility>> GetAllFacilitiesAsync();
    Task<Facility?> GetFacilityByIdAsync(int id);
    Task<Facility?> CreateFacilityAsync(CreateFacilityDto dto);
    Task<Facility?> UpdateFacilityAsync(int id, UpdateFacilityDto dto);
    Task<bool> DeleteFacilityAsync(int id);
}