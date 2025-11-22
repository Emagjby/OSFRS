using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

/// <summary>
/// Provides user-specific data access operations, including lookup
/// and uniqueness checks for email and username fields.
/// </summary>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email to search for.</param>
    /// <returns>
    /// The matching <see cref="User"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>
    /// The matching <see cref="User"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Retrieves a user by either their username or email.
    /// </summary>
    /// <param name="usernameOrEmail">A username or email string to search for.</param>
    /// <returns>
    /// The matching <see cref="User"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

    /// <summary>
    /// Determines whether a given email address already exists in the system.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>
    /// <c>true</c> if the email exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Determines whether a given username already exists in the system.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>
    /// <c>true</c> if the username exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> UsernameExistsAsync(string username);
}