using OSFRS.Backend.Helpers.Usage;
using OSFRS.Backend.Validators.Usage;
using static OSFRS.UnitTests.TestUtils.ValidatorTestHelpers;
using Input = (
    string? eventType,
    int? userId,
    int? facilityId,
    System.DateTime? from,
    System.DateTime? to
);

namespace OSFRS.UnitTests.Validators;

public class UsageQueryValidatorTests
{
    private readonly UsageQueryValidator _validator = new();

    // ============================================================
    // USER ID VALIDATION
    // ============================================================

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Validate_ShouldThrow_WhenUserIdInvalid(int badId)
    {
        Input input = (eventType: null, userId: badId, facilityId: null, from: null, to: null);

        await ShouldThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenUserIdValid()
    {
        Input input = (eventType: null, userId: 10, facilityId: null, from: null, to: null);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenUserIdNull()
    {
        Input input = (eventType: null, userId: null, facilityId: null, from: null, to: null);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    // ============================================================
    // FACILITY ID VALIDATION
    // ============================================================

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Validate_ShouldThrow_WhenFacilityIdInvalid(int badId)
    {
        Input input = (eventType: null, userId: null, facilityId: badId, from: null, to: null);

        await ShouldThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenFacilityIdValid()
    {
        Input input = (eventType: null, userId: null, facilityId: 15, from: null, to: null);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenFacilityIdNull()
    {
        Input input = (eventType: null, userId: null, facilityId: null, from: null, to: null);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    // ============================================================
    // EVENT TYPE VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenEventTypeInvalid()
    {
        Input input = ("BadEvent", null, null, null, null);

        await ShouldThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenEventTypeValid()
    {
        var validType = UsageEventTypes.All.First();

        Input input = (validType, null, null, null, null);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenEventTypeNull()
    {
        Input input = (eventType: null, userId: null, facilityId: null, from: null, to: null);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    // ============================================================
    // DATE RANGE VALIDATION
    // ============================================================

    [Fact]
    public async Task Validate_ShouldThrow_WhenFromGreaterThanTo()
    {
        var from = DateTime.UtcNow.AddDays(1);
        var to = DateTime.UtcNow;

        Input input = (eventType: null, userId: null, facilityId: null, from, to);

        await ShouldThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenFromEqualsTo()
    {
        var time = DateTime.UtcNow;

        Input input = (eventType: null, userId: null, facilityId: null, from: time, to: time);

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenOnlyFromProvided()
    {
        Input input = (
            eventType: null,
            userId: null,
            facilityId: null,
            from: DateTime.UtcNow,
            to: null
        );

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenOnlyToProvided()
    {
        Input input = (
            eventType: null,
            userId: null,
            facilityId: null,
            from: null,
            to: DateTime.UtcNow
        );

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenValidRangeProvided()
    {
        Input input = (
            eventType: null,
            userId: null,
            facilityId: null,
            from: DateTime.UtcNow.AddDays(-1),
            to: DateTime.UtcNow
        );

        await ShouldNotThrowValidation(() => _validator.ValidateAsync(input));
    }
}
