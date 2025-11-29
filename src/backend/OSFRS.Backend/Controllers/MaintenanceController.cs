using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/maintenance")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _service;
    private readonly IUsageService _usage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceController"/>.
    /// </summary>
    /// <param name="service">The service handling maintenance operations.</param>
    /// <param name="usage">The service used for audit logging of maintenance events.</param>
    public MaintenanceController(IMaintenanceService service, IUsageService usage)
    {
        _service = service;
        _usage = usage;
    }

    /// <summary>
    /// Retrieves maintenance records with optional filtering.
    /// </summary>
    /// <param name="status">
    /// Optional maintenance status filter (e.g. <c>Scheduled</c>, <c>InProgress</c>,
    /// <c>Completed</c>, <c>Cancelled</c>). If omitted, all statuses are included.
    /// </param>
    /// <param name="facilityId">
    /// Optional facility ID filter. If omitted, results include all facilities.
    /// </param>
    /// <returns>A filtered list of maintenance records.</returns>
    /// <response code="200">Returns matching maintenance records.</response>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetFilteredMaintenance(
        [FromQuery] string? status,
        [FromQuery] int? facilityId
    )
    {
        var records = await _service.GetFilteredMaintenanceAsync(status, facilityId);
        return Ok(records);
    }

    /// <summary>
    /// Retrieves all maintenance records for a specific facility.
    /// </summary>
    /// <param name="facilityId">The ID of the facility.</param>
    /// <returns>A list of maintenance records.</returns>
    /// <response code="200">Returns all maintenance records for the facility.</response>
    /// <response code="404">Facility does not exist.</response>
    [HttpGet("facility/{facilityId}")]
    public async Task<IActionResult> GetMaintenanceByFacility(int facilityId)
    {
        var records = await _service.GetMaintenanceByFacilityAsync(facilityId);
        return Ok(records);
    }

    /// <summary>
    /// Retrieves all upcoming (future) maintenance records.
    /// </summary>
    /// <returns>All future scheduled maintenance records.</returns>
    /// <response code="200">Upcoming maintenance returned successfully.</response>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingMaintenance()
    {
        var records = await _service.GetUpcomingMaintenanceAsync();
        return Ok(records);
    }

    /// <summary>
    /// Schedules a new maintenance record.
    /// </summary>
    /// <param name="dto">Details for the new maintenance entry.</param>
    /// <returns>The created maintenance record.</returns>
    /// <remarks>
    /// Only administrators can schedule new maintenance.
    /// A usage audit event is generated automatically.
    /// </remarks>
    /// <response code="200">Maintenance scheduled successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Facility does not exist.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ScheduleMaintenance([FromBody] CreateMaintenanceRecordDto dto)
    {
        var created = await _service.ScheduleMaintenanceAsync(dto);

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(
                eventType: UsageEventTypes.MaintenanceScheduled,
                userId: null,
                facilityId: created.FacilityId
            )
        );

        return Ok(created);
    }

    /// <summary>
    /// Updates an existing maintenance record.
    /// </summary>
    /// <param name="id">The ID of the maintenance record.</param>
    /// <param name="dto">Updated maintenance data.</param>
    /// <returns>The updated maintenance record.</returns>
    /// <remarks>
    /// Only administrators can update maintenance records.
    /// A usage audit event is generated automatically.
    /// </remarks>
    /// <response code="200">Maintenance updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Maintenance record not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMaintenance(
        int id,
        [FromBody] UpdateMaintenanceRecordDto dto
    )
    {
        var updated = await _service.UpdateMaintenanceAsync(id, dto);

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(
                eventType: UsageEventTypes.MaintenanceUpdated,
                userId: null,
                facilityId: updated!.FacilityId
            )
        );

        return Ok(updated);
    }

    /// <summary>
    /// Deletes a maintenance record.
    /// </summary>
    /// <param name="id">The ID of the maintenance record to delete.</param>
    /// <returns>A confirmation message.</returns>
    /// <remarks>
    /// Only administrators can delete maintenance records.
    /// A usage audit event is logged when deletion occurs.
    /// </remarks>
    /// <response code="200">Maintenance record deleted successfully.</response>
    /// <response code="404">Maintenance record not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMaintenance(int id)
    {
        var deleted = await _service.DeleteMaintenanceAsync(id);
        if (!deleted)
            return NotFound(new { message = "Maintenance record not found." });

        await _usage.LogEventAsync(UsageEventBuilder.Create(UsageEventTypes.MaintenanceDeleted));

        return Ok(new { message = "Maintenance record deleted successfully." });
    }

    /// <summary>
    /// Triggers a full synchronization of maintenance records and facility statuses.
    /// </summary>
    /// <returns>A confirmation message.</returns>
    /// <remarks>
    /// This endpoint recalculates maintenance states (Scheduled, InProgress, Completed)
    /// and updates related facilities accordingly, marking them as
    /// <c>UnderMaintenance</c> when an active maintenance window is detected or
    /// reverting them to <c>Available</c> when no such window exists.
    ///
    /// Only administrators are authorized to perform this operation.
    /// </remarks>
    /// <response code="200">Synchronization completed successfully.</response>
    [HttpPost("sync-statuses")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncStatuses()
    {
        await _service.SyncStatusesAsync();

        await _usage.LogEventAsync(UsageEventBuilder.Create(UsageEventTypes.StatusSyncRun));

        return Ok(new { message = "Facility statuses synchronized successfully." });
    }
}
