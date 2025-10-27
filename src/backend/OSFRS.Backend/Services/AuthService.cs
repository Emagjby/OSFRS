using OSFRS.Backend.DTOs;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Validators;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenGenerator _jwtGenerator;

    public AuthService(UserRepository userRepository, PasswordHasher passwordHasher, JwtTokenGenerator jwtGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<string> LoginAsync(LoginRequestDto dto)
    {
        // Validate credentials
        if (!UserValidator.ValidateUsername(dto.UsernameOrEmail)
            && !UserValidator.ValidateEmail(dto.UsernameOrEmail)) throw new ArgumentException("Invalid username or email.");
        if (!UserValidator.ValidatePassword(dto.Password)) throw new ArgumentException("Invalid password.");

        // Fetch user
        User? user = await _userRepository.GetByUsernameOrEmailAsync(dto.UsernameOrEmail);
        if (user == null) throw new Exception("Invalid credentials.");

        // Compare password
        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash)) throw new Exception("Invalid credentials.");

        // Generate JWT
        string token = _jwtGenerator.GenerateToken(user);

        return token;
    }
}