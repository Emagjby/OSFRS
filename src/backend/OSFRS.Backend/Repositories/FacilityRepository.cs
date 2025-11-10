using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class FacilityRepository : IFacilityRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<FacilityRepository> _logger; 

    public FacilityRepository(OSFRSDbContext context, IAppLogger<FacilityRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Facility> AddAsync(Facility facility)
    {
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Facility '{Name}' added successfully", facility.Name);

        return facility;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var facility = await _context.Facilities.FindAsync(id);
        if(facility == null)
        {
            _logger.LogWarning("Attempted to delete non-existant facility with ID '{Id}'.", id);
            return false;
        }

        _context.Facilities.Remove(facility);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Facility with ID '{Id}' deleted successfully", facility.Id);
        return true;
    }

    public async Task<IEnumerable<Facility>> GetAllAsync()
    {
        return await _context.Facilities.ToListAsync();
    }

    public async Task<Facility?> GetByIdAsync(int id)
    {
        return await _context.Facilities.FindAsync(id);
    }

    public async Task<bool> IsFacilityAvailableAsync(int facilityId)
    {
        var facility = await _context.Facilities.FindAsync(facilityId);
        if (facility == null)
            throw new InvalidOperationException($"Facility with ID {facilityId} not found.");

        return facility.Status == "Available";
    }

    public async Task<Facility> UpdateAsync(Facility facility)
    {
        var existant = await _context.Facilities.FindAsync(facility.Id);
        if (existant == null)
        {
            _logger.LogWarning("Attempted to update non-existant facility with ID {Id}", facility.Id);
            throw new InvalidOperationException("Facility not found.");
        }

        existant.Name = facility.Name;
        existant.Type = facility.Type;
        existant.Capacity = facility.Capacity;
        existant.Status = facility.Status;
        existant.UpdatedAt = DateTime.UtcNow; 

        await _context.SaveChangesAsync();
        _logger.LogInformation("Facility '{Name}' updated successfully.");

        return existant;
    }

    public async Task UpdateAvailabilityAsync(int facilityId, bool isAvailable)
    {
        var facility = await _context.Facilities.FindAsync(facilityId);
        if (facility == null)
        {
            _logger.LogWarning("Attempted to update availability of non-existent facility with ID {Id}.", facilityId);
            throw new InvalidOperationException("Facility not found.");
        }

        facility.Status = isAvailable ? "Available" : "Unavailable";
        facility.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Facility with ID '{Id}' availability updated to '{Status}'.", facilityId, facility.Status);
    }
}