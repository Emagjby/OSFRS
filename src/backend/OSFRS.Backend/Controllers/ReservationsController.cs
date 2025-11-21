using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.Helpers.Auth;
using Microsoft.AspNetCore.Authorization;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.DTOs.Reservations;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly IUsageService _usage;

    public ReservationsController(IReservationService service, IUsageService usage)
    {
        _service = service;
        _usage = usage;
    }

    [HttpGet("availability/{facilityId}")]
    public async Task<IActionResult> GetAvailabilityCalendar(int facilityId, [FromQuery] DateTime? date)
    {
        var calendar = await _service.GetAvailabilityCalendarAsync(facilityId, date);
        return Ok(calendar);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("facility/{facilityId}")]
    public async Task<IActionResult> GetReservations(int facilityId, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var reservations = await _service.GetReservationsAsync(facilityId, start, end);
        return Ok(reservations);
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
        var results = await _service.SearchReservationAsync(userId, facilityId, start, end);
        return Ok(results);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
    {
        var userId = UserContextHelper.GetUserId(User)!.Value;

        var reservation = await _service.CreateReservationAsync(dto, userId);

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(
                UsageEventTypes.ReservationCreated,
                userId: userId,
                facilityId: reservation.FacilityId,
                metadata: new() { { "ReservationId", reservation.Id.ToString() } }
            )
        );

        return Ok(reservation);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto dto)
    {
        var userId = UserContextHelper.GetUserId(User)!.Value;

        var updated = await _service.UpdateReservationAsync(id, dto, userId);

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(
                UsageEventTypes.ReservationUpdated,
                userId: userId,
                facilityId: updated.FacilityId,
                metadata: new() { { "ReservationId", updated.Id.ToString() } }
            )
        );

        return Ok(updated);
    }

    [HttpPut("cancel/{id}")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var userId = UserContextHelper.GetUserId(User)!.Value;

        await _service.CancelReservationAsync(id, userId);

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(
                UsageEventTypes.ReservationCancelled,
                userId: userId,
                metadata: new() { { "ReservationId", id.ToString() } }
            )
        );

        return Ok(new { message = "Reservation cancelled successfully." });
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations()
    {
        var userId = UserContextHelper.GetUserId(User);

        var reservations = await _service.SearchReservationAsync(userId: userId);
        return Ok(reservations);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllReservations()
    {
        var reservations = await _service.GetAllReservationsAsync();
        return Ok(reservations);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/update/{id}")]
    public async Task<IActionResult> AdminUpdateReservation(int id, [FromBody] UpdateReservationDto dto)
    {
        var adminId = UserContextHelper.GetUserId(User)!.Value;

        var updated = await _service.AdminUpdateReservationAsync(id, dto, adminId);

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(
                UsageEventTypes.ReservationAdminUpdated,
                userId: adminId,
                facilityId: updated.FacilityId,
                metadata: new() { { "ReservationId", id.ToString() } }
            )
        );

        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> AdminDeleteReservation(int id)
    {
        var adminId = UserContextHelper.GetUserId(User)!.Value;

        await _service.DeleteReservationAsync(id, adminId);

        await _usage.LogEventAsync(
            UsageEventBuilder.Create(
                UsageEventTypes.ReservationDeleted,
                userId: adminId,
                metadata: new() { { "ReservationId", id.ToString() } }
            )
        );

        return Ok(new { message = "Reservation deleted successfully." });
    }
}