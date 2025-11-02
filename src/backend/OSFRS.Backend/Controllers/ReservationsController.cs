using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Validators;
using OSFRS.Backend.Helpers.Auth;
using Microsoft.AspNetCore.Authorization;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly IAppLogger<ReservationsController> _logger;

    public ReservationsController(IReservationService reservationService, IAppLogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    [HttpGet("availability/{facilityId}")]
    public async Task<IActionResult> GetAvailabilityCalendar(int facilityId, [FromQuery] DateTime? date)
    {
        try
        {
            if (!ReservationValidator.ValidateFacilityId(facilityId)) return BadRequest("Invalid facility ID.");

            var calendar = await _reservationService.GetAvailabilityCalendarAsync(facilityId, date);
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

    [HttpGet("facility/{facilityId}")]
    public async Task<IActionResult> GetReservations(int facilityId, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        try
        {
            if (!ReservationValidator.ValidateFacilityId(facilityId)) return BadRequest("Invalid facility ID.");

            var reservations = await _reservationService.GetReservationsAsync(facilityId, start, end);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations for facility {FacilityId}", facilityId);
            return StatusCode(500, "Internal server error while fetching reservation.");
        }
    }

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
            var results = await _reservationService.SearchReservationAsync(userId, facilityId, start, end);
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

            dto.UserId = userId.Value;

            var reservation = await _reservationService.CreateReservationAsync(dto);
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

            var updatedReservation = await _reservationService.UpdateReservationAsync(id, dto, userId.Value);
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

    [HttpDelete("cancel/{id}")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(int id)
    {
        try
        {
            if (!ReservationValidator.ValidateReservationId(id)) return BadRequest("Invalid reservation ID.");

            var userId = UserContextHelper.GetUserId(User);
            if (userId == null) return Unauthorized("User ID not found in token.");

            await _reservationService.CancelReservationAsync(id, userId.Value);
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
}