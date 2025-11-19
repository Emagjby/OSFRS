using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService authService)
    {
        _service = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto dto)
    {
        try
        {
            await _service.RegisterUserAsync(dto);
            return Ok(new { message = "User registered successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            var token = await _service.LoginAsync(dto);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}