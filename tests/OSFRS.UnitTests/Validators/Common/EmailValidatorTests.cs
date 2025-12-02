using FluentAssertions;
using OSFRS.Backend.Validators.Common;

namespace OSFRS.UnitTests.Validators.Common;

public class EmailValidatorTests
{
    // ============================================================
    // NULL / EMPTY / WHITESPACE
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_ShouldReturnFalse_ForNullOrEmpty(string? input)
    {
        EmailValidator.IsValid(input).Should().BeFalse();
    }

    // ============================================================
    // MISSING PARTS
    // ============================================================

    [Theory]
    [InlineData("test")]
    [InlineData("test@")]
    [InlineData("@domain.com")]
    [InlineData("test@domain")]
    [InlineData("test@domain.")]
    public void IsValid_ShouldReturnFalse_ForMissingParts(string input)
    {
        EmailValidator.IsValid(input).Should().BeFalse();
    }

    // ============================================================
    // INVALID CHARACTERS
    // ============================================================

    [Theory]
    [InlineData("t e s t@a.com")]
    [InlineData("test@do main.com")]
    [InlineData("test@@domain.com")]
    public void IsValid_ShouldReturnFalse_ForInvalidCharacters(string input)
    {
        EmailValidator.IsValid(input).Should().BeFalse();
    }

    // ============================================================
    // VALID EMAILS
    // ============================================================

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@DOMAIN.COM")]
    [InlineData("user.name@domain.co")]
    [InlineData("u@d.io")]
    [InlineData("x_y+z@domain.info")]
    [InlineData("john-doe@dept.company.org")]
    public void IsValid_ShouldReturnTrue_ForValidEmails(string input)
    {
        EmailValidator.IsValid(input).Should().BeTrue();
    }
}
