using OSFRS.Backend.DTOs;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Validators;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly IAppLogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtGenerator, IAppLogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
        _logger = logger;
    }

    public async Task<string> LoginAsync(LoginRequestDto dto)
    {
        _logger.LogInformation("Login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);

        // Validate credentials
        if (!UserValidator.ValidateUsername(dto.UsernameOrEmail)
            && !UserValidator.ValidateEmail(dto.UsernameOrEmail)) 
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);
            throw new ArgumentException("Invalid username or email.");
        }
        if (!UserValidator.ValidatePassword(dto.Password)) 
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);
            throw new ArgumentException("Invalid password.");
        }

        // Fetch user
        User? user = await _userRepository.GetByUsernameOrEmailAsync(dto.UsernameOrEmail);
        if (user == null) 
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);
            throw new Exception("Invalid credentials.");
        }

        // Compare password
        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash)) 
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);
            throw new Exception("Invalid credentials.");
        }

        _logger.LogInformation("Login successful for {Username}", user.Username);

        // Generate JWT
        string token = _jwtGenerator.GenerateToken(user);

        return token;
    }
}