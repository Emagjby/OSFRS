using OSFRS.Backend.Validators.Facilities;
using OSFRS.UnitTests.TestUtils;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;

namespace OSFRS.UnitTests.Validators;

public class CreateFacilityValidatorTests
{
    private readonly CreateFacilityValidator _validator;

    public CreateFacilityValidatorTests()
    {
        _validator = new CreateFacilityValidator();
    }

    // ============================================================
    // NAME VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameEmpty()
    {
        var dto = FakeData.CreateFacilityDto().RuleFor(f => f.Name, _ => "").Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenNameTooLong()
    {
        var dto = FakeData
            .CreateFacilityDto()
            .RuleFor(f => f.Name, _ => new string('A', 101))
            .Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenNameValid()
    {
        var dto = FakeData.CreateFacilityDto().RuleFor(f => f.Name, _ => "Sports Hall").Generate();

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ============================================================
    // TYPE VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenTypeEmpty()
    {
        var dto = FakeData.CreateFacilityDto().RuleFor(f => f.Type, _ => "").Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldThrow_WhenTypeTooLong()
    {
        var dto = FakeData
            .CreateFacilityDto()
            .RuleFor(f => f.Type, _ => new string('T', 51))
            .Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenTypeValid()
    {
        var dto = FakeData.CreateFacilityDto().RuleFor(f => f.Type, _ => "Gym").Generate();

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ============================================================
    // CAPACITY VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenCapacityNotPositive()
    {
        var dto = FakeData.CreateFacilityDto().RuleFor(f => f.Capacity, _ => 0).Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCapacityPositive()
    {
        var dto = FakeData.CreateFacilityDto().RuleFor(f => f.Capacity, _ => 20).Generate();

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto));
    }

    // ============================================================
    // STATUS VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenStatusInvalid()
    {
        var dto = FakeData
            .CreateFacilityDto()
            .RuleFor(f => f.Status, _ => "CursedStatus")
            .Generate();

        await ShouldThrowValidation(() => _validator.ValidateAsync(dto));
    }

    [Theory]
    [InlineData("Available")]
    [InlineData("Unavailable")]
    [InlineData("UnderMaintenance")]
    public async Task Validate_ShouldPass_WhenStatusAllowed(string status)
    {
        var dto = FakeData.CreateFacilityDto().RuleFor(f => f.Status, _ => status).Generate();

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(dto));
    }
}
