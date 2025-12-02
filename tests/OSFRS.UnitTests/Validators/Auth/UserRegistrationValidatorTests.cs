using Moq;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Validators.Auth;
using OSFRS.UnitTests.TestUtils;

namespace OSFRS.UnitTests.Validators;

public class UserRegistrationValidatorTests
{
    private readonly Mock<IUserRepository> _repo;
    private readonly UserRegistrationValidator _validator;

    public UserRegistrationValidatorTests()
    {
        _repo = MockFactories.UserRepo();
        _validator = new UserRegistrationValidator(_repo.Object);
    }

    // ============================================================
    // NAME VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameMissing()
    {
        var dto = FakeData.UserRegistrationDto().RuleFor(x => x.Name, _ => "").Generate();
        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameHasNumbers()
    {
        var dto = FakeData.UserRegistrationDto().RuleFor(x => x.Name, _ => "John123").Generate();
        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameHasSymbols()
    {
        var dto = FakeData.UserRegistrationDto().RuleFor(x => x.Name, _ => "John@Doe").Generate();
        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameHasDoubleSpaces()
    {
        var dto = FakeData.UserRegistrationDto().RuleFor(x => x.Name, _ => "John  Doe").Generate();
        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameTooLong()
    {
        var longName = new string('a', 51);
        var dto = FakeData.UserRegistrationDto().RuleFor(x => x.Name, _ => longName).Generate();

        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenNameValid()
    {
        var dto = FakeData.UserRegistrationDto().RuleFor(x => x.Name, _ => "John Doe").Generate();
        await ValidatorTestHelpers.ShouldNotThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ============================================================
    // USERNAME VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenUsernameInvalid()
    {
        var dto = FakeData
            .UserRegistrationDto()
            .RuleFor(x => x.Username, _ => "bad??name")
            .Generate();
        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenUsernameExists()
    {
        var dto = FakeData.UserRegistrationDto().Generate();

        _repo.Setup(r => r.UsernameExistsAsync(dto.Username)).ReturnsAsync(true);

        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ============================================================
    // EMAIL VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenEmailInvalid()
    {
        var dto = FakeData.UserRegistrationDto().RuleFor(x => x.Email, _ => "invalid").Generate();
        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenEmailExists()
    {
        var dto = FakeData.UserRegistrationDto().Generate();

        _repo.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(true);

        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ============================================================
    // PASSWORD VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenPasswordWeak()
    {
        var dto = FakeData.UserRegistrationDto().RuleFor(x => x.Password, _ => "1234").Generate();
        await ValidatorTestHelpers.ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ============================================================
    // VALID DTO SUCCESSFUL
    // ============================================================

    [Fact]
    public async Task Validate_ShouldPass_WhenDtoValid()
    {
        var dto = FakeData.UserRegistrationDto().Generate();

        _repo.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _repo.Setup(r => r.UsernameExistsAsync(dto.Username)).ReturnsAsync(false);

        await ValidatorTestHelpers.ShouldNotThrowValidation(() => _validator.ValidateAsync(dto));
    }
}
