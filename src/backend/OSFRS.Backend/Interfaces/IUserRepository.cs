using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> GetByIdAsync(int id);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}