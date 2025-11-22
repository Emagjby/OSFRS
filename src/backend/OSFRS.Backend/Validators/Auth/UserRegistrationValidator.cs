using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Backend.Validators.Common;

namespace OSFRS.Backend.Validators.Auth;

/// <summary>
/// Validates user registration data by enforcing formatting, strength rules,
/// and ensuring uniqueness of usernames and emails.
/// </summary>
public class UserRegistrationValidator : BaseValidator, IValidator<UserRegistrationDto>
{
    private readonly IUserRepository _repo;

    /// <summary>
    /// Initializes a new instance of the validator with access to the user repository
    /// for uniqueness checks.
    /// </summary>
    /// <param name="repo">Repository used for username and email lookup.</param>
    public UserRegistrationValidator(IUserRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Performs validation for a user registration request, ensuring:
    /// <list type="bullet">
    /// <item><description>Name follows formatting and length requirements.</description></item>
    /// <item><description>Username format is valid and unique.</description></item>
    /// <item><description>Email format is valid and unique.</description></item>
    /// <item><description>Password meets strength requirements.</description></item>
    /// </list>
    /// </summary>
    /// <param name="dto">Registration request data.</param>
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