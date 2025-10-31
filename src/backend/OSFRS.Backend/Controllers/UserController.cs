using Microsoft.AspNetCore.Mvc;
using OSFRS.Backend.Services;
using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;
using OSFRS.Backend.Interfaces;

namespace OSFRS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    //POST: api/user/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto dto)
    {
        try
        {
            await _userService.RegisterUserAsync(dto);
            return Ok(new { message = "User registered successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    
}