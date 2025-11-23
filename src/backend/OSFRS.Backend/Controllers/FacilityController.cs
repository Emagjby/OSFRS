using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.Helpers.Auth;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/facility")]
public class FacilityController : ControllerBase
{
    private readonly IFacilityService _facility;
    private readonly IUsageService _usage;

    /// <summary>
    /// Initializes a new instance of the <see cref="FacilityController"/>.
    /// </summary>
    /// <param name="facility">The facility service used to manage facility data.</param>
    /// <param name="usage">The usage service used for audit logging.</param>
    public FacilityController(IFacilityService facility, IUsageService usage)
    {
        _facility = facility;
        _usage = usage;
    }

    /// <summary>
    /// Retrieves a list of all facilities.
    /// </summary>
    /// <returns>A collection of facility records.</returns>
    /// <response code="200">Returns the full list of facilities.</response>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var facilities = await _facility.GetAllAsync();
        return Ok(facilities);
    }

    /// <summary>
    /// Retrieves a facility by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the facility.</param>
    /// <returns>The facility record if found.</returns>
    /// <response code="200">Facility found and returned.</response>
    /// <response code="404">Facility not found.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var facility = await _facility.GetByIdAsync(id);
        if (facility is null)
            return NotFound(new { message = $"Facility with ID {id} not found." });

        return Ok(facility);
    }

    /// <summary>
    /// Creates a new facility.
    /// </summary>
    /// <param name="dto">The facility creation payload.</param>
    /// <returns>The newly created facility.</returns>
    /// <remarks>
    /// Only administrators are allowed to create new facilities.
    /// A usage event is automatically logged after creation.
    /// </remarks>
    /// <response code="201">Facility created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">User is not authorized.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFacilityDto dto)
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

    /// <summary>
    /// Updates an existing facility.
    /// </summary>
    /// <param name="id">The identifier of the facility to update.</param>
    /// <param name="dto">The update payload containing modified fields.</param>
    /// <returns>The updated facility.</returns>
    /// <remarks>
    /// Only administrators can update facilities.
    /// A usage event is logged after a successful update.
    /// </remarks>
    /// <response code="200">Facility updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Facility not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFacilityDto dto)
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

    /// <summary>
    /// Deletes a facility.
    /// </summary>
    /// <param name="id">The identifier of the facility to delete.</param>
    /// <returns>A confirmation message.</returns>
    /// <remarks>
    /// Only administrators can delete facilities.
    /// A usage audit event is logged after successful deletion.
    /// </remarks>
    /// <response code="200">Facility deleted successfully.</response>
    /// <response code="404">Facility not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _facility.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Facility with ID {id} not found." });

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(
                UsageEventTypes.FacilityDeleted,
                userId: UserContextHelper.GetUserId(User),
                metadata: new() { { "FacilityId", id.ToString() } }
            )
        );

        return Ok(new { message = "Facility deleted successfully." });
    }

    /// <summary>
    /// Retrieves the availability status of a facility.
    /// </summary>
    /// <param name="id">The facility identifier.</param>
    /// <returns>Whether the facility is currently available.</returns>
    /// <remarks>
    /// Restricted to administrators.
    /// </remarks>
    /// <response code="200">Availability status returned.</response>
    [HttpGet("{id}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAvailability(int id)
    {
        var available = await _facility.IsFacilityAvailableAsync(id);
        return Ok(new { FacilityId = id, IsAvailable = available });
    }

    /// <summary>
    /// Updates the availability of a facility.
    /// </summary>
    /// <param name="id">The facility identifier.</param>
    /// <param name="availability">The new availability state.</param>
    /// <returns>A confirmation message.</returns>
    /// <remarks>
    /// Logs an audit event describing the availability change.
    /// Only administrators may modify availability.
    /// </remarks>
    /// <response code="200">Availability updated successfully.</response>
    /// <response code="404">Facility not found.</response>
    [HttpPatch("{id}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAvailability(int id, [FromBody] bool availability)
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
}