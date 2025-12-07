using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Validators.Auth;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class UserLoginValidatorTests
{
    private readonly UserLoginValidator _validator = new();

    // ----------------------------------------------------------
    // MISSING USERNAME OR EMAIL
    // ----------------------------------------------------------
    [Fact]
    public async Task Validate_ShouldThrow_WhenUsernameOrEmailMissing()
    {
        var dto = new LoginRequestDto { UsernameOrEmail = "", Password = "pass1234" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ----------------------------------------------------------
    // INVALID USERNAME OR EMAIL FORMAT
    // ----------------------------------------------------------
    [Fact]
    public async Task Validate_ShouldThrow_WhenInvalidUsernameOrEmailFormat()
    {
        var dto = new LoginRequestDto { UsernameOrEmail = "???", Password = "pass1234" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ----------------------------------------------------------
    // MISSING PASSWORD
    // ----------------------------------------------------------
    [Fact]
    public async Task Validate_ShouldThrow_WhenPasswordMissing()
    {
        var dto = new LoginRequestDto { UsernameOrEmail = "validUser123", Password = "" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ----------------------------------------------------------
    // VALID DTO
    // ----------------------------------------------------------
    [Fact]
    public async Task Validate_ShouldNotThrow_WhenValid()
    {
        var dto = new LoginRequestDto
        {
            UsernameOrEmail = "validUser123",
            Password = "StrongPass!123",
        };

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto));
    }
}
