using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;

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
            if (facilityId <= 0) return BadRequest("Invalid facility ID.");

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
            if (facilityId <= 0) return BadRequest("Invalid facility ID.");

            var reservations = await _reservationService.GetReservationsAsync(facilityId, start, end);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations for facility {FacilityId}", facilityId);
            return StatusCode(500, "Internal server error while fetching availability.");
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
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error searching reservations.");
            return StatusCode(500, "Internal server error while searching reservations.");
        }
    }
}