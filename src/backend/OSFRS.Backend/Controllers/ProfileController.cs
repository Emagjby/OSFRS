using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OSFRS.Backend.Helpers.Auth;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.DTOs.Auth;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _service;

    public ProfileController(IProfileService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = UserContextHelper.GetUserId(User)!.Value;

        try
        {
            var profile = await _service.GetProfileAsync(userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatedProfileDto dto)
    {
        var userId = UserContextHelper.GetUserId(User)!.Value;

        try
        {
            await _service.UpdateProfileAsync(userId, dto);
            return Ok(new { message = "Profile updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}