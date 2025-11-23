using OSFRS.Backend.Data;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

/// <summary>
/// Repository implementation for <see cref="Facility"/> entities, extending the generic
/// <see cref="BaseRepository{Facility}"/> with facility-specific operations such as
/// availability checks and availability updates.
/// </summary>
public class FacilityRepository : BaseRepository<Facility>, IFacilityRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FacilityRepository"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework database context.</param>
    /// <param name="logger">The logger used for repository-level diagnostics.</param>
    public FacilityRepository(
        OSFRSDbContext context,
        IAppLogger<BaseRepository<Facility>> logger
    ) : base(context, logger)
    {
    }

    /// <summary>
    /// Checks whether a facility is currently marked as available.
    /// </summary>
    /// <param name="facilityId">The facility identifier.</param>
    /// <returns>
    /// <c>true</c> if the facility status equals <c>"Available"</c>, otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the facility does not exist.</exception>
    public async Task<bool> IsFacilityAvailableAsync(int facilityId)
    {
        _logger.LogInformation("Checking availability for Facility ID {FacilityId}...", facilityId);

        var facility = await _dbSet.FindAsync(facilityId)
            ?? throw new NotFoundException("Facility not found.");

        bool available = facility!.Status == "Available";

        _logger.LogInformation(
            "Facility ID {FacilityId} availability = {Available}.",
            facilityId,
            available
        );

        return available;
    }

    /// <summary>
    /// Updates a facility's availability status by toggling the <see cref="Facility.Status"/> property.
    /// </summary>
    /// <param name="facilityId">The facility identifier.</param>
    /// <param name="isAvailable">Whether the facility should be marked available.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the facility does not exist.</exception>
    public async Task UpdateAvailabilityAsync(int facilityId, bool isAvailable)
    {
        _logger.LogInformation("Updating availability for Facility ID {Id}", facilityId);

        var facility = await _dbSet.FindAsync(facilityId)
            ?? throw new NotFoundException("Facility not found.");

        facility!.Status = isAvailable ? "Available" : "Unavailable";
        facility.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Facility ID {Id} availability updated to {Status}",
            facilityId,
            facility.Status
        );
    }
}