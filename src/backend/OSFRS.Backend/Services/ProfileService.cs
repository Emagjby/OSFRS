using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Models.Entities;
using OSFRS.Backend.Exceptions;

namespace OSFRS.Backend.Services;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _pass;
    private readonly IAppLogger<ProfileService> _logger;
    private readonly IUpdateValidator<UpdatedProfileDto, User> _validator;

    public ProfileService(
        IUserRepository repo,
        IPasswordHasher hasher,
        IAppLogger<ProfileService> logger,
        IUpdateValidator<UpdatedProfileDto, User> validator
    )
    {
        _repo = repo;
        _pass = hasher;
        _logger = logger;
        _validator = validator;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user is null)
        {
            throw new Exception("User not found.");
        }

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
        _logger.LogInformation("Updating profile for User {UserId}", userId);

        var user = await _repo.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User not found.");

        await _validator.ValidateAsync(dto, user);

        if (dto.Name is not null)
            user.Name = dto.Name;

        if (dto.Username is not null)
            user.Username = dto.Username;

        if (dto.Email is not null)
            user.Email = dto.Email;

        if (dto.Password is not null)
            user.PasswordHash = _pass.Hash(dto.Password);

        user.UpdatedAt = DateTime.UtcNow;

        _repo.Update(user);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Profile updated successfully for User {UserId}", userId);
    }
}