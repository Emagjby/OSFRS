using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Validators.Base;
using OSFRS.Backend.Validators.Common;
using OSFRS.Models.Entities;

namespace OSFRS.Backend.Validators.Auth;

public class ProfileUpdateValidator : BaseValidator, IUpdateValidator<UpdatedProfileDto, User>
{
    private readonly IUserRepository _repo;

    public ProfileUpdateValidator(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task ValidateAsync(UpdatedProfileDto dto, User existing)
    {
        // NAME
        if (dto.Name is not null)
        {
            Require(!string.IsNullOrWhiteSpace(dto.Name.Trim()), "Name is required.");
            Require(dto.Name.Length <= 50, "Name cannot exceed 50 characters.");
            Require(dto.Name.Trim().All(c => char.IsLetter(c) || c == ' '),
                "Name can only contain letters and spaces.");
            Require(!dto.Name.Contains("  "), "Name cannot contain multiple consecutive spaces.");
        }

        // USERNAME
        if (dto.Username is not null)
        {
            Require(UsernameValidator.IsValid(dto.Username), "Invalid username format.");

            if (dto.Username != existing.Username)
            {
                Require(!await _repo.UsernameExistsAsync(dto.Username), "Username already in use.");
            }
        }

        // EMAIL
        if (dto.Email is not null)
        {
            Require(EmailValidator.IsValid(dto.Email), "Invalid email format.");

            if (dto.Email != existing.Email)
            {
                Require(!await _repo.EmailExistsAsync(dto.Email), "Email already in use.");
            }
        }

        // PASSWORD
        if (dto.Password is not null)
        {
            Require(PasswordValidator.IsStrong(dto.Password), "Password is too weak.");
        }
    }
}