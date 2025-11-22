using OSFRS.Backend.DTOs.Auth;

namespace OSFRS.Backend.Interfaces.Service;

/// <summary>
/// Provides operations for retrieving and updating a user's profile.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Retrieves the full profile information for a user.
    /// </summary>
    /// <param name="userId">The identifier of the user whose profile is being fetched.</param>
    /// <returns>
    /// A <see cref="UserProfileDto"/> containing profile details.
    /// </returns>
    Task<UserProfileDto> GetProfileAsync(int userId);

    /// <summary>
    /// Updates a user's profile information.
    /// </summary>
    /// <param name="userId">The identifier of the user to update.</param>
    /// <param name="dto">The new profile data.</param>
    Task UpdateProfileAsync(int userId, UpdatedProfileDto dto);
}