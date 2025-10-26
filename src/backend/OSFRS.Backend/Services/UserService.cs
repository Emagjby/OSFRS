using OSFRS.Backend.Repositories;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Validators;
using OSFRS.Backend.DTOs;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class UserService
{
    private readonly UserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;

    public UserService(UserRepository userRepository, PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task RegisterUserAsync(UserRegistrationDto dto)
    {
        // Validation
        if (!UserValidator.ValidateName(dto.Name)) throw new ArgumentException("Invalid name.");
        if (!UserValidator.ValidateUsername(dto.Username)) throw new ArgumentException("Invalid username.");
        if (!UserValidator.ValidateEmail(dto.Email)) throw new ArgumentException("Invalid email.");
        if (!UserValidator.ValidatePassword(dto.Password)) throw new ArgumentException("Invalid password.");

        // Check Duplicates
        if (await _userRepository.EmailExistsAsync(dto.Email)) throw new InvalidOperationException("Email address is already in use.");
        if (await _userRepository.UsernameExistsAsync(dto.Username)) throw new InvalidOperationException("Username is already in use.");

        // Assign Default Role
        var role = "User";

        // Hash Password
        var passwordHash = _passwordHasher.Hash(dto.Password);

        // Create entity
        var user = new User
        {
            Name = dto.Name,
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Save to db
        await _userRepository.AddUserAsync(user);
    }
}