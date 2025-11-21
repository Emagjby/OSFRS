using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Exceptions;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Backend.Validators.Common;

namespace OSFRS.Backend.Validators.Auth;

public class UserLoginValidator : BaseValidator, IValidator<LoginRequestDto>
{
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