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
/// Provides read and update operations for user profile information.
/// Handles profile field modification, password updates, and validation.
/// </summary>
public class ProfileService : IProfileService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _pass;
    private readonly IAppLogger<ProfileService> _logger;
    private readonly IUpdateValidator<UpdatedProfileDto, User> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileService"/> class.
    /// </summary>
    /// <param name="repo">Repository used for querying and updating user records.</param>
    /// <param name="hasher">Password hashing abstraction.</param>
    /// <param name="logger">Logging abstraction for profile operations.</param>
    /// <param name="validator">Validator enforcing update rules and business logic.</param>
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

    /// <summary>
    /// Retrieves a user's profile by their unique identifier.
    /// </summary>
    /// <param name="userId">The ID of the user whose profile is being requested.</param>
    /// <returns>A <see cref="UserProfileDto"/> containing full profile information.</returns>
    /// <exception cref="Exception">Thrown when the specified user does not exist.</exception>
    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
        };
    }

    /// <summary>
    /// Updates editable fields of a user's profile
    /// </summary>
    /// <param name="userId">The ID of the user whose profile is being modified.</param>
    /// <param name="dto">Update request containing modified fields.</param>
    /// <exception cref="NotFoundException">Thrown when the user cannot be found.</exception>
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

        user.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        _logger.LogInformation("Profile updated successfully for User {UserId}", userId);
    }
}
