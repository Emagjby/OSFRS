using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Helpers.Auth;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FacilityController : ControllerBase
{
    private readonly IFacilityService _facility;
    private readonly IUsageService _usage;

    public FacilityController(IFacilityService facility, IUsageService usage)
    {
        _facility = facility;
        _usage = usage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var facilities = await _facility.GetAllAsync();
        return Ok(facilities);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var facility = await _facility.GetByIdAsync(id);
        if (facility is null)
            return NotFound(new { message = $"Facility with ID {id} not found." });

        return Ok(facility);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFacilityDto dto)
    {
        try
        {
            var created = await _facility.CreateAsync(dto);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    UsageEventTypes.FacilityCreated,
                    userId: UserContextHelper.GetUserId(User),
                    facilityId: created.Id
                )
            );

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFacilityDto dto)
    {
        try
        {
            var updated = await _facility.UpdateAsync(id, dto);

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
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _facility.DeleteAsync(id);
            if (!deleted)
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
        catch
        {
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{id}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAvailability(int id)
    {
        try
        {
            var available = await _facility.IsFacilityAvailableAsync(id);
            return Ok(new { FacilityId = id, IsAvailable = available });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAvailability(int id, [FromBody] bool availability)
    {
        try
        {
            await _facility.UpdateAvailabilityAsync(id, availability);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    UsageEventTypes.FacilityAvailabilityChanged,
                    userId: UserContextHelper.GetUserId(User),
                    facilityId: id,
                    metadata: new Dictionary<string, string>
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
    }
}