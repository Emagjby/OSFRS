using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _service;
    private readonly IUsageService _usage;

    public MaintenanceController(IMaintenanceService service, IUsageService usage)
    {
        _service = service;
        _usage = usage;
    }


    [HttpGet("facility/{facilityId}")]
    public async Task<IActionResult> GetMaintenanceByFacility(int facilityId)
    {
        var records = await _service.GetMaintenanceByFacilityAsync(facilityId);
        return Ok(records);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingMaintenance()
    {
        var records = await _service.GetUpcomingMaintenanceAsync();
        return Ok(records);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ScheduleMaintenance([FromBody] CreateMaintenanceRecordDto dto)
    {
        var created = await _service.ScheduleMaintenanceAsync(dto);

        await _usage.LogEventAsync(UsageEventBuilder.Create(
            eventType: UsageEventTypes.MaintenanceScheduled,
            userId: null,
            facilityId: created.FacilityId
        ));

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMaintenance(int id, [FromBody] UpdateMaintenanceRecordDto dto)
    {
        var updated = await _service.UpdateMaintenanceAsync(id, dto);

        await _usage.LogEventAsync(UsageEventBuilder.Create(
            eventType: UsageEventTypes.MaintenanceUpdated,
            userId: null,
            facilityId: updated!.FacilityId
        ));

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMaintenance(int id)
    {
        var deleted = await _service.DeleteMaintenanceAsync(id);
        if (!deleted)
            return NotFound(new { message = "Maintenance record not found." });

        await _usage.LogEventAsync(UsageEventBuilder.Create(
            UsageEventTypes.MaintenanceDeleted
        ));

        return Ok(new { message = "Maintenance record deleted successfully." });
    }

    [HttpPost("sync-statuses")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncStatuses()
    {
        await _service.SyncFacilityStatusesAsync();

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(UsageEventTypes.StatusSyncRun)
        );

        return Ok(new { message = "Facility statuses synchronized successfully." });
    }
}