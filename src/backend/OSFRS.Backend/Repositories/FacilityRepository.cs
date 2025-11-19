using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class FacilityRepository : BaseRepository<Facility>, IFacilityRepository
{
    public FacilityRepository(
        OSFRSDbContext context,
        IAppLogger<BaseRepository<Facility>> logger
    ) : base (context, logger)
    {
        
    }

    public async Task<bool> IsFacilityAvailableAsync(int facilityId)
    {
        _logger.LogInformation("Checking availability for Facility ID {FacilityId}...", facilityId);

        var facility = await _dbSet.FindAsync(facilityId);

        if (facility == null)
        {
            _logger.LogWarning("Facility ID {Id} not found", facilityId);
            throw new InvalidOperationException($"Facility with ID {facilityId} not found.");
        }

        bool available = facility.Status == "Available";

        _logger.LogInformation("Facility ID {FacilityId} availability = {Available}.", facilityId, available);
        return available;
    }

    public async Task UpdateAvailabilityAsync(int facilityId, bool isAvailable)
    {
        _logger.LogInformation("Updating availability for Facility ID {Id}", facilityId);

        var facility = await _dbSet.FindAsync(facilityId);
        if (facility == null)
        {
            _logger.LogWarning("Attempted to update availability of non-existent facility with ID {Id}.", facilityId);
            throw new InvalidOperationException("Facility not found.");
        }

        facility.Status = isAvailable ? "Available" : "Unavailable";
        facility.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Facility ID {Id} availability updated to {Status}", 
            facilityId, facility.Status
        );
    }
}