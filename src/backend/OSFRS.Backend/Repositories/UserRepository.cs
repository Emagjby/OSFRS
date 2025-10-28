using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class UserRepository : IUserRepository
{
    private readonly OSFRSDbContext _context;
    private readonly IAppLogger<UserRepository> _logger;

    public UserRepository(OSFRSDbContext context, IAppLogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Get all users
    public async Task<List<User>> GetAllAsync()
    {
        var users = await _context.Users.ToListAsync();
        _logger.LogInformation("Fetched all users. Count: {Count}", users.Count);
        return users;
    }

    // Get user by email address
    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
        if (user != null)
        {
            _logger.LogInformation("Fetched user by email: {Email}", email);
        }
        else
        {
            _logger.LogInformation("No user found with email: {Email}", email);
        }
        return user;
    }

    // Get user by username
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
        if (user != null)
        {
            _logger.LogInformation("Fetched user by username: {Username}", username);
        }
        else
        {
            _logger.LogInformation("No user found with username: {Username}", username);
        }
        return user;
    }

    // Get user by username or email
    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        // No logging here as not requested in instructions
        return await _context.Users.FirstOrDefaultAsync(user => user.Username == usernameOrEmail || user.Email == usernameOrEmail);
    }
    
    // Get user by ID
    public async Task<User?> GetByIdAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (user != null)
        {
            _logger.LogInformation("Fetched user by ID: {Id}", id);
        }
        else
        {
            _logger.LogInformation("No user found with ID: {Id}", id);
        }
        return user;
    }

    // Add new user
    public async Task AddUserAsync(User user)
    {
        _context.Users.Add(user);
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created new user with ID: {Id}, Username: {Username}, Email: {Email}", user.Id, user.Username, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user with Username: {Username}, Email: {Email}", user.Username, user.Email);
            throw;
        }
    }

    // Update user
    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated user with ID: {Id}, Username: {Username}, Email: {Email}", user.Id, user.Username, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with ID: {Id}, Username: {Username}, Email: {Email}", user.Id, user.Username, user.Email);
            throw;
        }
    }

    // Checks if email exists
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(user => user.Email == email);
    }

    // Checks if username exists
    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(user => user.Username == username);
    }
}