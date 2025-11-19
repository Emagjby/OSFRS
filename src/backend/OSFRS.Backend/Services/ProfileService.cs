using OSFRS.Backend.DTOs;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Validators;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Helpers.Analytics;

namespace OSFRS.Backend.Services;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _pass;
    private readonly IAppLogger<ProfileService> _logger;

    public ProfileService(
        IUserRepository repo,
        IPasswordHasher hasher,
        IAppLogger<ProfileService> logger
    )
    {
        _repo = repo;
        _pass = hasher;
        _logger = logger;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user is null)
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
            var user = await _repo.GetByIdAsync(userId);
            if (user is null) throw new Exception("User not found.");

            if (!UserValidator.ValidateName(dto.Name)) throw new ArgumentException("Invalid name.");
            if (!UserValidator.ValidateUsername(dto.Username)) throw new ArgumentException("Invalid username.");
            if (!UserValidator.ValidateEmail(dto.Email)) throw new ArgumentException("Invalid email.");
            if (dto.Password is not null && !UserValidator.ValidatePassword(dto.Password)) throw new ArgumentException("Invalid password.");

            if (!string.Equals(user.Username, dto.Username, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _repo.GetByUsernameAsync(dto.Username);
                if (existingUser is not null && existingUser.Id != user.Id)
                    throw new InvalidOperationException("Username is already taken.");
            }

            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _repo.GetByEmailAsync(dto.Email);
                if (existingUser is not null && existingUser.Id != user.Id)
                    throw new InvalidOperationException("Email is already taken.");
            }

            user.Name = dto.Name;
            user.Username = dto.Username;
            user.Email = dto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = _pass.Hash(dto.Password);

            _repo.Update(user);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Profile updated successfully for userId: {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Profile update failed for userId: {userId}");
            throw;
        }
    }
}