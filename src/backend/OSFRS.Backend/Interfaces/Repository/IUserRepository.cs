using OSFRS.Backend.Interfaces.Base;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Interfaces.Repository;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}