using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IUsageService _service;
    private readonly IAppLogger<StatisticsController> _logger;

    public StatisticsController(IUsageService service, IAppLogger<StatisticsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? eventType,
        [FromQuery] int? userId,
        [FromQuery] int? facilityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
        {
            var events = await _service.GetEventsAsync(
                eventType,
                userId,
                facilityId,
                from,
                to
            );
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching usage events.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("aggregate/daily")]
    public async Task<IActionResult> GetDailyAggregate([FromQuery] DateTime? date)
    {
        try
        {
            var targetDate = date ?? DateTime.UtcNow;

            var results = await _service.GetDailyAggregateAsync(targetDate);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching daily analytics.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("aggregate/monthly")]
    public async Task<IActionResult> GetMonthlyAggregate([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            int targetYear = year ?? DateTime.UtcNow.Year;
            int targetMonth = month ?? DateTime.UtcNow.Month;

            var results = await _service.GetMonthlyAggregateAsync(targetYear, targetMonth);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching monthly analytics.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("aggregate/run")]
    public async Task<IActionResult> RunAggregation()
    {
        try
        {
            await _service.AggregateAsync();
            return Ok(new { message = "Aggregation executed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running aggregation job.");
            return StatusCode(500, new { message = "Internal server error." });
        }
    }
}