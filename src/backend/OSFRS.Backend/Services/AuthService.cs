using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Validators;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IAppLogger<AuthService> _logger;

    public AuthService(
        IUserRepository repo,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IAppLogger<AuthService> logger) 
    {
        _repo = repo;
        _hasher = hasher;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<string> LoginAsync(LoginRequestDto dto)
    {
        _logger.LogInformation("Login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);

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

        User? user = await _repo.GetByUsernameOrEmailAsync(dto.UsernameOrEmail);
        if (user == null)
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);
            throw new Exception("Invalid credentials.");
        }

        if (!_hasher.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);
            throw new Exception("Invalid credentials.");
        }

        _logger.LogInformation("Login successful for {Username}", user.Username);

        string token = _jwt.GenerateToken(user);

        return token;
    }
    
    public async Task RegisterUserAsync(UserRegistrationDto dto)
    {
        try
        {
            _logger.LogInformation("Starting user registration for {Email}", dto.Email);

            if (!UserValidator.ValidateName(dto.Name)) throw new ArgumentException("Invalid name.");
            if (!UserValidator.ValidateUsername(dto.Username)) throw new ArgumentException("Invalid username.");
            if (!UserValidator.ValidateEmail(dto.Email)) throw new ArgumentException("Invalid email.");
            if (!UserValidator.ValidatePassword(dto.Password)) throw new ArgumentException("Invalid password.");

            if (await _repo.EmailExistsAsync(dto.Email)) throw new InvalidOperationException("Email address is already in use.");
            if (await _repo.UsernameExistsAsync(dto.Username)) throw new InvalidOperationException("Username is already in use.");

            var role = "User";

            var passwordHash = _hasher.Hash(dto.Password);

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

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("User {Email} registered successfully.", dto.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration for {Email}", dto.Email);
            throw;
        }
    }
}