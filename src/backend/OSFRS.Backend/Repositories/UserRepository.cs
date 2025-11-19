using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{

    public UserRepository(
        OSFRSDbContext context,
        IAppLogger<BaseRepository<User>> logger
    ) : base(context, logger)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = await _dbSet.FirstOrDefaultAsync(user => user.Email == email);

        if (user is not null)
            _logger.LogInformation("Fetched user by email: {Email}", email);
        else
            _logger.LogInformation("No user found with email: {Email}", email);

        return user;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var user = await _dbSet.FirstOrDefaultAsync(user => user.Username == username);

        if (user is not null)
            _logger.LogInformation("Fetched user by username: {Username}", username);
        else
            _logger.LogInformation("No user found with username: {Username}", username);
        
        return user;
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _dbSet
            .FirstOrDefaultAsync(user =>
                user.Username == usernameOrEmail ||
                user.Email == usernameOrEmail
            );
    }
    
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

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(user => user.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(user => user.Username == username);
    }
}