using OSFRS.Backend.DTOs;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Validators;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Services;

public class ProfileService : IProfileService
{
    private readonly UserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;
    private readonly IAppLogger<ProfileService> _logger;

    public ProfileService(UserRepository userRepository, PasswordHasher passwordHasher, IAppLogger<ProfileService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"Profile fetch failed: user {userId} not found.");
            throw new Exception("User not found.");
        }

        _logger.LogInformation($"Profile fetched for userId: {userId}");

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task UpdateProfileAsync(int userId, UpdatedProfileDto dto)
    {
        _logger.LogInformation($"Starting profile update for userId: {userId}");
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            // Validate data
            if (!UserValidator.ValidateName(dto.Name)) throw new ArgumentException("Invalid name.");
            if (!UserValidator.ValidateUsername(dto.Username)) throw new ArgumentException("Invalid username.");
            if (!UserValidator.ValidateEmail(dto.Email)) throw new ArgumentException("Invalid email.");
            if (dto.Password is not null && !UserValidator.ValidatePassword(dto.Password)) throw new ArgumentException("Invalid password.");

            // Check for duplicates
            if (user.Username != dto.Username)
            {
                var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
                if (existingUser != null && existingUser.Id != user.Id)
                    throw new Exception("Username is already taken.");
            }

            if (user.Email != dto.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                    throw new Exception("Email is already taken.");
            }

            // Update props
            user.Name = dto.Name;
            user.Username = dto.Username;
            user.Email = dto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            // Hash pass if provided
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = _passwordHasher.Hash(dto.Password);

            await _userRepository.UpdateUserAsync(user);
            _logger.LogInformation($"Profile updated successfully for userId: {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Profile update failed for userId: {userId}");
            throw;
        }
    }
}