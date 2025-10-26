using Microsoft.EntityFrameworkCore;
using OSFRS.Backend.Data;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Repositories;

public class UserRepository
{
    private readonly OSFRSDbContext _context;

    public UserRepository(OSFRSDbContext context)
    {
        _context = context;
    }

    // Get all users
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    // Get user by email address
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    // Get user by username
    public async Task<User?> GetByUsername(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
    }

    // Get user by username or email
    public async Task <User?> GetByUsernameOrEmail(string usernameOrEmail)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Username == usernameOrEmail || user.Email == usernameOrEmail);
    }

    // Add new user
    public async Task AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
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