using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Validators;
using OSFRS.Backend.Helpers.Auth;
using Microsoft.AspNetCore.Authorization;
using OSFRS.Backend.Helpers.Usage;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly IUsageService _usage;
    private readonly IAppLogger<ReservationsController> _logger;

    public ReservationsController(IReservationService service, IUsageService usage, IAppLogger<ReservationsController> logger)
    {
        _service = service;
        _usage = usage;
        _logger = logger;
    }

    [HttpGet("availability/{facilityId}")]
    public async Task<IActionResult> GetAvailabilityCalendar(int facilityId, [FromQuery] DateTime? date)
    {
        try
        {
            if (!ReservationValidator.ValidateFacilityId(facilityId)) return BadRequest("Invalid facility ID.");

            var calendar = await _service.GetAvailabilityCalendarAsync(facilityId, date);
            if (!calendar.Any())
            {
                _logger.LogInformation("No reservations found for facility {FacilityId} on {Date}", facilityId, date ?? DateTime.UtcNow);
                return NotFound("No reservations found for this facility on the specified date.");
            }

            return Ok(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving availability for facility {FacilityId}", facilityId);
            return StatusCode(500, "Internal server error while fetching availability.");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("facility/{facilityId}")]
    public async Task<IActionResult> GetReservations(int facilityId, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        try
        {
            if (!ReservationValidator.ValidateFacilityId(facilityId)) return BadRequest("Invalid facility ID.");

            var reservations = await _service.GetReservationsAsync(facilityId, start, end);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations for facility {FacilityId}", facilityId);
            return StatusCode(500, "Internal server error while fetching reservation.");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("search")]
    public async Task<IActionResult> SearchReservation(
        [FromQuery] int? userId,
        [FromQuery] int? facilityId,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end
    )
    {
        try
        {
            var results = await _service.SearchReservationAsync(userId, facilityId, start, end);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching reservations.");
            return StatusCode(500, "Internal server error while searching reservations.");
        }
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
    {
        try
        {
            var userId = UserContextHelper.GetUserId(User);
            if (userId == null) return Unauthorized("User ID not found in token.");

            var reservation = await _service.CreateReservationAsync(dto, userId.Value);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    eventType: UsageEventTypes.ReservationCreated,
                    userId: userId.Value,
                    facilityId: reservation.FacilityId,
                    metadata: new() { { "ReservationId", reservation.Id.ToString() } }
                )
            );

            return Ok(reservation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating reservation.");
            return StatusCode(500, "Internal server error while creating reservation.");
        }
    }

    [HttpPut("update/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto dto)
    {
        try
        {
            if (!ReservationValidator.ValidateUpdate(dto)) return BadRequest("Invalid reservation update data.");

            var userId = UserContextHelper.GetUserId(User);
            if (userId == null) return Unauthorized("User ID not found in token.");

            var updatedReservation = await _service.UpdateReservationAsync(id, dto, userId.Value);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    eventType: UsageEventTypes.ReservationUpdated,
                    userId: userId.Value,
                    facilityId: updatedReservation.FacilityId,
                    metadata: new() { { "ReservationId", updatedReservation.Id.ToString() } }
                )
            );

            return Ok(updatedReservation);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating reservation {ReservationId}", id);
            return StatusCode(500, "Internal server error while updating reservation.");
        }
    }

    [HttpPut("cancel/{id}")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(int id)
    {
        try
        {
            if (!ReservationValidator.ValidateReservationId(id)) return BadRequest("Invalid reservation ID.");

            var userId = UserContextHelper.GetUserId(User);
            if (userId == null) return Unauthorized("User ID not found in token.");

            await _service.CancelReservationAsync(id, userId.Value);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    eventType: UsageEventTypes.ReservationCancelled,
                    userId: userId.Value,
                    facilityId: null,
                    metadata: new() { { "ReservationId", id.ToString() } }
                )
            );

            return Ok(new { message = "Reservation cancelled successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error cancelling reservation {ReservationId}", id);
            return StatusCode(500, "Internal server error while cancelling reservation.");
        }
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations()
    {
        var userId = UserContextHelper.GetUserId(User);
        if (userId == null)
            return Unauthorized(new { message = "User ID not found in token." });
        
        try
        {
            var myReservations = await _service.SearchReservationAsync(userId, null, null, null);
            if (!myReservations.Any())
            {
                _logger.LogInformation("No reservations found for user {UserId}.", userId);
                return NotFound("You have no active or past reservations.");
            }
            
            _logger.LogInformation("User {UserId} fetched {Count} reservations.", userId, myReservations.Count());
            return Ok(myReservations);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid parameters while fetching reservations for user {UserId}.", userId);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Operation failed while fetching reservations for user {UserId}.", userId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching reservations for user {UserId}.", userId);
            return StatusCode(500, new { message = "Internal server error while fetching your reservations." });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllReservations()
    {
        try
        {
            var reservations = await _service.GetAllReservationsAsync();

            if (!reservations.Any())
            {
                _logger.LogInformation("No reservations found when fetching all (admin request).");
                return NotFound(new { message = "No reservations found in the system." });
            }

            _logger.LogInformation("Admin fetched {Count} total reservations.", reservations.Count());
            return Ok(reservations);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all reservations (admin request).");
            return StatusCode(500, new { message = "Internal server error while fetching reservations" });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/update/{id}")]
    public async Task<IActionResult> AdminUpdateReservation(int id, [FromBody] UpdateReservationDto dto)
    {
        try
        {
            if (!ReservationValidator.ValidateReservationId(id))
                return BadRequest(new { message = "Invalid reservation ID." });

            if (!ReservationValidator.ValidateUpdate(dto))
                return BadRequest(new { message = "Invalid reservation update data." });

            var adminId = UserContextHelper.GetUserId(User);
            if (adminId == null)
                return Unauthorized(new { message = "Admin ID not found in token." });

            var updatedReservation = await _service.AdminUpdateReservationAsync(id, dto, adminId.Value);

            _logger.LogInformation("Admin {AdminId} successfully updated reservation {ReservationId}.", adminId, id);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    eventType: UsageEventTypes.ReservationAdminUpdated,
                    userId: adminId.Value,
                    facilityId: updatedReservation.FacilityId,
                    metadata: new() { { "ReservationId", id.ToString() } }
                )
            );

            return Ok(updatedReservation);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while admin updating reservation {ReservationId}", id);
            return StatusCode(500, new { message = "Internal server error while updating reservation." });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("admin/delete/{id}")]
    public async Task<IActionResult> AdminDeleteReservation(int id)
    {
        try
        {
            if (!ReservationValidator.ValidateReservationId(id))
                return BadRequest(new { message = "Invalid reservation ID." });

            var adminId = UserContextHelper.GetUserId(User);
            if (adminId == null)
            {
                return Unauthorized(new { message = "Admin ID not found in token." });
            }

            await _service.DeleteReservationAsync(id, adminId.Value);

            _logger.LogInformation("Admin {AdminId} successfully deleted reservation {ReservationId}.", adminId, id);

            await _usage.LogEventAsync(
                UsageEventBuilder.Create(
                    eventType: UsageEventTypes.ReservationDeleted,
                    userId: adminId.Value,
                    facilityId: null,
                    metadata: new() { { "ReservationId", id.ToString() } }
                )
            );

            return Ok(new { message = "Reservation deleted successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting reservation {ReservationId} by admin.", id);
            return StatusCode(500, new { message = "Internal server error while deleting reservation." });
        }
    }
}