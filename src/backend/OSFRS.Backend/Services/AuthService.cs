using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

/// <summary>
/// Provides authentication operations including user registration
/// and credential-based login with JWT issuance.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IAppLogger<AuthService> _logger;

    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<UserRegistrationDto> _registrationValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="repo">User repository instance.</param>
    /// <param name="hasher">Password hashing abstraction.</param>
    /// <param name="jwt">JWT token generator.</param>
    /// <param name="logger">Logging abstraction.</param>
    /// <param name="loginValidator">Validator for login requests.</param>
    /// <param name="registrationValidator">Validator for registration requests.</param>
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

    /// <summary>
    /// Authenticates a user using username or email and password.
    /// Returns a signed JWT token on success.
    /// </summary>
    /// <param name="dto">Login credentials.</param>
    /// <returns>A signed JWT token if authentication succeeds.</returns>
    /// <exception cref="ValidationException">Thrown when credentials are invalid.</exception>
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

        if (!_hasher.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt for {UsernameOrEmail} - wrong password", dto.UsernameOrEmail);
            throw new ValidationException("Invalid credentials.");
        }

        _logger.LogInformation("Login successful for {Username}", user.Username);

        var token = _jwt.GenerateToken(user);
        return token;
    }

    /// <summary>
    /// Registers a new user in the system after passing validation checks.
    /// </summary>
    /// <param name="dto">Registration data including username, email, and password.</param>
    /// <returns>A completed task when registration is stored in the database.</returns>
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