using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Helpers.Auth;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileController"/>.
    /// </summary>
    /// <param name="service">The profile service handling user profile operations.</param>
    public ProfileController(IProfileService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves the profile of the currently authenticated user.
    /// </summary>
    /// <returns>The user's profile details.</returns>
    /// <remarks>
    /// The user ID is extracted from the JWT token via <see cref="UserContextHelper"/>.
    /// </remarks>
    /// <response code="200">Returns the profile of the authenticated user.</response>
    /// <response code="401">Invalid or missing authentication token.</response>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = UserContextHelper.GetUserId(User)!.Value;

        var profile = await _service.GetProfileAsync(userId);
        return Ok(profile);
    }

    /// <summary>
    /// Updates the profile information of the currently authenticated user.
    /// </summary>
    /// <param name="dto">The updated profile fields.</param>
    /// <returns>A confirmation message.</returns>
    /// <remarks>
    /// Validation and uniqueness checks are handled in <see cref="IProfileService"/>.
    /// </remarks>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Validation failed (e.g. invalid email, username, etc.).</response>
    /// <response code="409">Username or email already taken.</response>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatedProfileDto dto)
    {
        var userId = UserContextHelper.GetUserId(User)!.Value;

        await _service.UpdateProfileAsync(userId, dto);
        return Ok(new { message = "Profile updated successfully." });
    }
}
