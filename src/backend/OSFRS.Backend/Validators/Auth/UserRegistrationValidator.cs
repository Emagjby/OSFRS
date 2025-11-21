using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Backend.Validators.Common;

namespace OSFRS.Backend.Validators.Auth;

public class UserRegistrationValidator : BaseValidator, IValidator<UserRegistrationDto>
{
    private readonly IUserRepository _repo;

    public UserRegistrationValidator(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task ValidateAsync(UserRegistrationDto dto)
    {
        // NAME
        Require(!string.IsNullOrWhiteSpace(dto.Name.Trim()), "Name is required.");
        Require(dto.Name.Length <= 50, "Name cannot exceed 50 characters.");
        Require(dto.Name.Trim().All(c => char.IsLetter(c) || c == ' '),
            "Name can only contain letters and spaces.");
        Require(!dto.Name.Contains("  "), "Name cannot contain multiple consecutive spaces.");

        // USERNAME
        Require(!string.IsNullOrWhiteSpace(dto.Username), "Username is required.");
        Require(UsernameValidator.IsValid(dto.Username), "Invalid username format.");

        // EMAIL
        Require(EmailValidator.IsValid(dto.Email), "Invalid email format.");

        // PASSWORD
        Require(PasswordValidator.IsStrong(dto.Password), "Password is too weak.");

        // UNIQUE CHECK
        Require(!await _repo.EmailExistsAsync(dto.Email),
            "Email already in use.");

        Require(!await _repo.UsernameExistsAsync(dto.Username),
            "Username already in use.");
    }
}