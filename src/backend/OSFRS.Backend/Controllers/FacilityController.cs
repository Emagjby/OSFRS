using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs;
using OSFRS.Backend.Helpers.Auth;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacilityController : ControllerBase
{
    private readonly IFacilityService _service;
    private readonly IUsageService _usage;
    private readonly IAppLogger<FacilityController> _logger;

    public FacilityController(IFacilityService service, IUsageService usage, IAppLogger<FacilityController> logger)
    {
        _service = service;
        _usage = usage;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var facilities = await _service.GetAllFacilitiesAsync();
            if (!facilities.Any())
                return NotFound(new { message = "No facilities found." });

            return Ok(facilities);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching facilities.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var facility = await _service.GetFacilityByIdAsync(id);
            if (facility == null)
                return NotFound(new { message = $"Facility with ID {id} not found." });

            return Ok(facility);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching facility {Id}", id);
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFacilityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateFacilityAsync(dto);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    UsageEventTypes.FacilityCreated,
                    userId: UserContextHelper.GetUserId(User),
                    facilityId: created!.Id
                )
            );  

            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating facility.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFacilityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateFacilityAsync(id, dto);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    UsageEventTypes.FacilityUpdated,
                    userId: UserContextHelper.GetUserId(User),
                    facilityId: id
                )
            );

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating facility {Id}", id);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteFacilityAsync(id);
            if (!success)
                return NotFound(new { message = $"Facility with ID {id} not found." });

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    UsageEventTypes.FacilityDeleted,
                    userId: UserContextHelper.GetUserId(User),
                    facilityId: id
                )
            );

            return Ok(new { message = "Facility deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting facility {Id}", id);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{id}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAvailability(int id)
    {
        try
        {
            var available = await _service.IsFacilityAvailableAsync(id);
            return Ok(new { FacilityId = id, IsAvailable = available });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for facility {Id}", id);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPatch("{id}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAvailability(int id, [FromBody] bool availability)
    {
        try
        {
            await _service.UpdateAvailabilityAsync(id, availability);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    UsageEventTypes.FacilityAvailabilityChanged,
                    userId: UserContextHelper.GetUserId(User),
                    facilityId: id,
                    metadata: new Dictionary<string,string>
                    {
                        ["NewAvailability"] = availability.ToString()
                    }
                )
            );

            return Ok(new { message = "Facility availability updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for Facility {Id}", id);
            return StatusCode(500, "Internal server error.");
        }
    }
}