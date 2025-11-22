using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Backend.Validators.Common;

namespace OSFRS.Backend.Validators.Auth;

/// <summary>
/// Validates login requests by ensuring the username/email and password
/// follow required formatting and are present.
/// </summary>
public class UserLoginValidator : BaseValidator, IValidator<LoginRequestDto>
{
    /// <summary>
    /// Validates a login request ensuring both identity and password fields are properly provided
    /// and formatted as a valid username or email.
    /// </summary>
    /// <param name="dto">The login request payload.</param>
    public Task ValidateAsync(LoginRequestDto dto)
    {
        Require(!string.IsNullOrWhiteSpace(dto.UsernameOrEmail),
            "Username or email is required.");

        bool validUser = UsernameValidator.IsValid(dto.UsernameOrEmail);
        bool validEmail = EmailValidator.IsValid(dto.UsernameOrEmail);

        Require(validUser || validEmail,
            "Must be a valid username or email.");

        Require(!string.IsNullOrWhiteSpace(dto.Password),
            "Password is required.");

        return Task.CompletedTask;
    }
}