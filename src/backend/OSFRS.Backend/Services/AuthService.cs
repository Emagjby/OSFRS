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
    
    public async Task RegisterUserAsync(UserRegistrationDto dto)
    {
        try
        {
            _logger.LogInformation("Starting user registration for {Email}", dto.Email);

            // Validation
            if (!UserValidator.ValidateName(dto.Name)) throw new ArgumentException("Invalid name.");
            if (!UserValidator.ValidateUsername(dto.Username)) throw new ArgumentException("Invalid username.");
            if (!UserValidator.ValidateEmail(dto.Email)) throw new ArgumentException("Invalid email.");
            if (!UserValidator.ValidatePassword(dto.Password)) throw new ArgumentException("Invalid password.");

            // Check Duplicates
            if (await _userRepository.EmailExistsAsync(dto.Email)) throw new InvalidOperationException("Email address is already in use.");
            if (await _userRepository.UsernameExistsAsync(dto.Username)) throw new InvalidOperationException("Username is already in use.");

            // Assign Default Role
            var role = "User";

            // Hash Password
            var passwordHash = _passwordHasher.Hash(dto.Password);

            // Create entity
            var user = new User
            {
                Name = dto.Name,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save to db
            await _userRepository.AddUserAsync(user);

            _logger.LogInformation("User {Email} registered successfully.", dto.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration for {Email}", dto.Email);
            throw;
        }
    }
}