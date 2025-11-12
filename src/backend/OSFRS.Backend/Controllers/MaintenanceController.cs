using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _service;
    private readonly IAppLogger<MaintenanceController> _logger;

    public MaintenanceController(IMaintenanceService service, IAppLogger<MaintenanceController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ScheduleMaintenance([FromBody] CreateMaintenanceRecordDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.ScheduleMaintenanceAsync(dto);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to schedule maintenance due to invalid state.");
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error while scheduling maintenance.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while scheduling maintenance.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMaintenance(int id, [FromBody] UpdateMaintenanceRecordDto dto)
    {        
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateMaintenanceAsync(id, dto);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error while updating maintenance record {Id}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Attempted to update non-existent maintenance record {Id}.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating maintenance record {Id}.", id);
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMaintenance(int id)
    {
        try
        {
            var deleted = await _service.DeleteMaintenanceAsync(id);
            if (!deleted)
                return NotFound(new { message = "Maintenance record not found." });

            return Ok(new { message = "Maintenance record deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting maintenance record {Id}.", id);
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpGet("facility/{facilityId}")]
    [Authorize]
    public async Task<IActionResult> GetMaintenanceByFacility(int facilityId)
    {
        try
        {
            var maintenanceRecords = await _service.GetMaintenanceByFacilityAsync(facilityId);
            if (!maintenanceRecords.Any())
                return NotFound(new { message = "No maintenance records found for this facility." });

            return Ok(maintenanceRecords);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Facility ID {FacilityId} not found.", facilityId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching maintenance records for facility {FacilityId}.", facilityId);
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpGet("upcoming")]
    [Authorize]
    public async Task<IActionResult> GetUpcomingMaintenance()
    {
        try
        {
            var maintenanceRecords = await _service.GetUpcomingMaintenanceAsync();
            if (!maintenanceRecords.Any())
                return NotFound(new { message = "No upcoming maintenance records found." });

            return Ok(maintenanceRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching upcoming maintenance records.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpPost("sync-statuses")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncStatuses()
    {
        try
        {
            await _service.SyncFacilityStatusesAsync();
            _logger.LogInformation("Manual facility status sync executed by admin.");
            return Ok(new { message = "Facility statuses synchronized successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual facility status synchronization.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }
}