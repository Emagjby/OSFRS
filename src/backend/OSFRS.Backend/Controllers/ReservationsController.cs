using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.Helpers.Auth;
using Microsoft.AspNetCore.Authorization;
using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.DTOs.Reservations;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly IUsageService _usage;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReservationsController"/>.
    /// </summary>
    /// <param name="service">Service handling reservation operations.</param>
    /// <param name="usage">Service responsible for usage event logging.</param>
    public ReservationsController(IReservationService service, IUsageService usage)
    {
        _service = service;
        _usage = usage;
    }

    /// <summary>
    /// Returns the availability calendar for a given facility and day.
    /// </summary>
    /// <param name="facilityId">The facility identifier.</param>
    /// <param name="date">Optional specific date to filter availability.</param>
    /// <returns>A list of availability slots.</returns>
    /// <response code="200">Returns calendar data.</response>
    [HttpGet("availability/{facilityId}")]
    public async Task<IActionResult> GetAvailabilityCalendar(int facilityId, [FromQuery] DateTime? date)
    {
        var utcDate = date.HasValue
            ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc)
            : DateTime.UtcNow.Date;

        var calendar = await _service.GetAvailabilityCalendarAsync(facilityId, utcDate);
        return Ok(calendar);
    }

    /// <summary>
    /// Returns all reservations for a facility within an optional date range.
    /// </summary>
    /// <param name="facilityId">The facility identifier.</param>
    /// <param name="start">Start of the date range.</param>
    /// <param name="end">End of the date range.</param>
    /// <returns>A list of reservations.</returns>
    /// <response code="200">Reservations successfully returned.</response>
    [Authorize(Roles = "Admin")]
    [HttpGet("facility/{facilityId}")]
    public async Task<IActionResult> GetReservations(int facilityId, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var reservations = await _service.GetReservationsAsync(facilityId, start, end);
        return Ok(reservations);
    }

    /// <summary>
    /// Searches reservations using flexible filters: user, facility, and time range.
    /// </summary>
    /// <returns>A filtered list of reservations.</returns>
    /// <response code="200">Search results returned.</response>
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

    /// <summary>
    /// Creates a reservation for the authenticated user.
    /// </summary>
    /// <param name="dto">Reservation creation data.</param>
    /// <returns>The created reservation.</returns>
    /// <response code="200">Reservation created.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="409">Slot conflict.</response>
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

    /// <summary>
    /// Updates a reservation owned by the authenticated user.
    /// </summary>
    /// <param name="id">Reservation identifier.</param>
    /// <param name="dto">Updated reservation information.</param>
    /// <returns>The updated reservation.</returns>
    /// <response code="200">Reservation updated.</response>
    /// <response code="403">User not allowed to modify this reservation.</response>
    /// <response code="400">Validation failure.</response>
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

    /// <summary>
    /// Cancels a reservation owned by the authenticated user.
    /// </summary>
    /// <param name="id">Reservation identifier.</param>
    /// <returns>A status message.</returns>
    /// <response code="200">Reservation cancelled.</response>
    /// <response code="403">User cannot cancel this reservation.</response>
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

    /// <summary>
    /// Returns all reservations belonging to the authenticated user.
    /// </summary>
    /// <returns>The user's reservations.</returns>
    /// <response code="200">Reservations returned.</response>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations()
    {
        var userId = UserContextHelper.GetUserId(User);

        var reservations = await _service.SearchReservationAsync(userId: userId);
        return Ok(reservations);
    }

    /// <summary>
    /// Returns all reservations in the system.
    /// </summary>
    /// <response code="200">All reservations returned.</response>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllReservations()
    {
        var reservations = await _service.GetAllReservationsAsync();
        return Ok(reservations);
    }

    /// <summary>
    /// Allows an admin to update any reservation and override conflicts.
    /// </summary>
    /// <param name="id">Reservation identifier.</param>
    /// <param name="dto">Updated reservation data.</param>
    /// <returns>The updated reservation.</returns>
    /// <response code="200">Reservation updated.</response>
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

    /// <summary>
    /// Allows an admin to delete any reservation.
    /// </summary>
    /// <param name="id">Reservation identifier.</param>
    /// <returns>A status message.</returns>
    /// <response code="200">Reservation deleted.</response>
    /// <response code="404">Reservation not found.</response>
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