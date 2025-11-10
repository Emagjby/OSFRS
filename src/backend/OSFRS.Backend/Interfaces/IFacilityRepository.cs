using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IFacilityRepository
{
    Task<IEnumerable<Facility>> GetAllAsync();
    Task<Facility?> GetByIdAsync(int id);
    Task<Facility> AddAsync(Facility facility);
    Task<Facility> UpdateAsync(Facility facility);
    Task<bool> DeleteAsync(int id);
}