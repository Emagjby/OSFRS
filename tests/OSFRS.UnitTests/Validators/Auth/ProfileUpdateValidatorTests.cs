using Moq;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Validators.Auth;
using OSFRS.UnitTests.TestUtils;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class ProfileUpdateValidatorTests
{
    private readonly Mock<IUserRepository> _repo;
    private readonly ProfileUpdateValidator _validator;

    public ProfileUpdateValidatorTests()
    {
        _repo = MockFactories.UserRepo();
        _validator = new ProfileUpdateValidator(_repo.Object);
    }

    // ============================================================
    // SUCCESS — EMPTY UPDATE
    // ============================================================

    [Fact]
    public async Task Validate_ShouldPass_WhenDtoIsEmpty()
    {
        var dto = new UpdatedProfileDto();

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    // ============================================================
    // NAME VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameHasInvalidCharacters()
    {
        var dto = new UpdatedProfileDto { Name = "John123" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameHasDoubleSpaces()
    {
        var dto = new UpdatedProfileDto { Name = "John  Smith" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameTooLong()
    {
        var dto = new UpdatedProfileDto { Name = new string('a', 51) };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenNameIsValid()
    {
        var dto = new UpdatedProfileDto { Name = "John Smith" };

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    // ============================================================
    // USERNAME VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenUsernameFormatInvalid()
    {
        var dto = new UpdatedProfileDto { Username = "??bad##" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenUsernameAlreadyExists()
    {
        var dto = new UpdatedProfileDto { Username = "newuser" };

        _repo.Setup(r => r.UsernameExistsAsync("newuser")).ReturnsAsync(true);

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    [Fact]
    public async Task Validate_ShouldNotCheckUniqueness_WhenUsernameUnchanged()
    {
        var dto = new UpdatedProfileDto { Username = ExistingUser.Username };

        // Repo should NOT be touched
        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));

        _repo.Verify(r => r.UsernameExistsAsync(It.IsAny<string>()), Times.Never);
    }

    // ============================================================
    // EMAIL VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenEmailFormatInvalid()
    {
        var dto = new UpdatedProfileDto { Email = "not-an-email" };

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenEmailAlreadyExists()
    {
        var dto = new UpdatedProfileDto { Email = "new@mail.com" };

        _repo.Setup(r => r.EmailExistsAsync("new@mail.com")).ReturnsAsync(true);

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }

    [Fact]
    public async Task Validate_ShouldNotCheckUniqueness_WhenEmailUnchanged()
    {
        var dto = new UpdatedProfileDto { Email = ExistingUser.Email };

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));

        _repo.Verify(r => r.EmailExistsAsync(It.IsAny<string>()), Times.Never);
    }

    // ============================================================
    // FULL VALID CASE
    // ============================================================

    [Fact]
    public async Task Validate_ShouldPass_WhenAllFieldsValid()
    {
        var dto = new UpdatedProfileDto
        {
            Name = "John Smith",
            Username = "valid_user123",
            Email = "new@mail.com",
        };

        _repo.Setup(r => r.UsernameExistsAsync("valid_user123")).ReturnsAsync(false);
        _repo.Setup(r => r.EmailExistsAsync("new@mail.com")).ReturnsAsync(false);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto, ExistingUser));
    }
}
