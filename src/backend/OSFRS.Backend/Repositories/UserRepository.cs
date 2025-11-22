using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

/// <summary>
/// Repository responsible for querying and managing <see cref="User"/> entities,
/// including lookup by email, username, or combined credentials,
/// alongside existence checks and standard CRUD operations inherited
/// from <see cref="BaseRepository{User}"/>.
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">Database context used for persistence and querying.</param>
    /// <param name="logger">Logger instance for repository-level diagnostics.</param>
    public UserRepository(
        OSFRSDbContext context,
        IAppLogger<BaseRepository<User>> logger
    ) : base(context, logger)
    {
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">Email to search for.</param>
    /// <returns>The matching <see cref="User"/> or null if not found.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = await _dbSet.FirstOrDefaultAsync(user => user.Email == email);

        if (user is not null)
            _logger.LogInformation("Fetched user by email: {Email}", email);
        else
            _logger.LogInformation("No user found with email: {Email}", email);

        return user;
    }

    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="username">Username to search for.</param>
    /// <returns>The matching <see cref="User"/> or null if not found.</returns>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var user = await _dbSet.FirstOrDefaultAsync(user => user.Username == username);

        if (user is not null)
            _logger.LogInformation("Fetched user by username: {Username}", username);
        else
            _logger.LogInformation("No user found with username: {Username}", username);

        return user;
    }

    /// <summary>
    /// Retrieves a user by either username or email. Useful during login.
    /// </summary>
    /// <param name="usernameOrEmail">Combined credential input.</param>
    /// <returns>The matching <see cref="User"/> or null if not found.</returns>
    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _dbSet.FirstOrDefaultAsync(user =>
            user.Username == usernameOrEmail ||
            user.Email == usernameOrEmail
        );
    }

    /// <summary>
    /// Retrieves a user by their ID, with logging feedback.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public override async Task<User?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user != null)
            _logger.LogInformation("Fetched user by ID: {Id}", id);
        else
            _logger.LogInformation("No user found with ID: {Id}", id);

        return user;
    }

    /// <summary>
    /// Checks whether a given email address is already registered.
    /// </summary>
    /// <param name="email">Email to verify.</param>
    /// <returns>True if the email exists, false otherwise.</returns>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(user => user.Email == email);
    }

    /// <summary>
    /// Checks whether a given username is already taken.
    /// </summary>
    /// <param name="username">Username to verify.</param>
    /// <returns>True if the username exists, false otherwise.</returns>
    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(user => user.Username == username);
    }
}