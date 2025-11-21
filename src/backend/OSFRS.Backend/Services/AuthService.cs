using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IAppLogger<AuthService> _logger;

    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<UserRegistrationDto> _registrationValidator;

    public AuthService(
        IUserRepository repo,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IAppLogger<AuthService> logger,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<UserRegistrationDto> registrationValidator)
    {
        _repo = repo;
        _hasher = hasher;
        _jwt = jwt;
        _logger = logger;
        _loginValidator = loginValidator;
        _registrationValidator = registrationValidator;
    }

    public async Task<string> LoginAsync(LoginRequestDto dto)
    {
        await _loginValidator.ValidateAsync(dto);

        _logger.LogInformation("Login attempt for {UsernameOrEmail}", dto.UsernameOrEmail);

        var user = await _repo.GetByUsernameOrEmailAsync(dto.UsernameOrEmail);
        if (user is null)
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail} - user not found", dto.UsernameOrEmail);
            throw new ValidationException("Invalid credentials.");
        }

        if (!_hasher.Verify(dto.Password, user!.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail} - wrong password", dto.UsernameOrEmail);
            throw new ValidationException("Invalid credentials.");
        }

        _logger.LogInformation("Login successful for {Username}", user.Username);

        var token = _jwt.GenerateToken(user);
        return token;
    }

    public async Task RegisterUserAsync(UserRegistrationDto dto)
    {
        _logger.LogInformation("Starting user registration for {Email}", dto.Email);

        await _registrationValidator.ValidateAsync(dto);

        var user = new User
        {
            Name = dto.Name,
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered successfully.", dto.Email);
    }
}