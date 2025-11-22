using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Interfaces.Service;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">The authentication service used to handle login and registration operations.</param>
    public AuthController(IAuthService authService)
    {
        _service = authService;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="dto">The payload containing user registration details.</param>
    /// <returns>
    /// A response indicating successful registration.
    /// </returns>
    /// <remarks>
    /// This endpoint creates a new user with the provided credentials.
    /// All validation is performed by the underlying authentication service.
    /// </remarks>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto dto)
    {
        await _service.RegisterUserAsync(dto);
        return Ok(new { message = "User registered successfully." });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="dto">The login credentials containing username/email and password.</param>
    /// <returns>
    /// A JWT token if authentication is successful.
    /// </returns>
    /// <remarks>
    /// This endpoint verifies user credentials and issues a signed JWT token.
    /// </remarks>
    /// <response code="200">Authentication successful and token returned.</response>
    /// <response code="400">Invalid credentials or validation error.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var token = await _service.LoginAsync(dto);
        return Ok(new { token });
    }
}