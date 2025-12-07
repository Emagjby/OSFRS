using FluentAssertions;
using OSFRS.Backend.Validators.Common;

namespace OSFRS.UnitTests.Validators.Common;

public class PasswordValidatorTests
{
    // ============================================================
    // NULL / EMPTY / WHITESPACE
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsStrong_ShouldReturnFalse_ForNullOrEmpty(string? input)
    {
        PasswordValidator.IsStrong(input).Should().BeFalse();
    }

    // ============================================================
    // TOO SHORT
    // ============================================================

    [Theory]
    [InlineData("Aa1!")]
    [InlineData("Aa1!!")]
    [InlineData("Ab1$cd")] // 6 chars
    public void IsStrong_ShouldReturnFalse_WhenTooShort(string input)
    {
        PasswordValidator.IsStrong(input).Should().BeFalse();
    }

    // ============================================================
    // MISSING LOWERCASE
    // ============================================================

    [Fact]
    public void IsStrong_ShouldReturnFalse_WhenMissingLowercase()
    {
        PasswordValidator.IsStrong("ABCDEF1!").Should().BeFalse();
    }

    // ============================================================
    // MISSING UPPERCASE
    // ============================================================

    [Fact]
    public void IsStrong_ShouldReturnFalse_WhenMissingUppercase()
    {
        PasswordValidator.IsStrong("abcdef1!").Should().BeFalse();
    }

    // ============================================================
    // MISSING DIGIT
    // ============================================================

    [Fact]
    public void IsStrong_ShouldReturnFalse_WhenMissingDigit()
    {
        PasswordValidator.IsStrong("Abcdefg!").Should().BeFalse();
    }

    // ============================================================
    // MISSING SYMBOL
    // ============================================================

    [Fact]
    public void IsStrong_ShouldReturnFalse_WhenMissingSymbol()
    {
        PasswordValidator.IsStrong("Abcdefg1").Should().BeFalse();
    }

    // ============================================================
    // VALID PASSWORDS
    // ============================================================

    [Theory]
    [InlineData("Aa1!aaaa")]
    [InlineData("XyZ9@word")]
    [InlineData("Pass123!")]
    [InlineData("Str0ng_Pass")]
    [InlineData("Valid99#Ab")]
    public void IsStrong_ShouldReturnTrue_ForValidPasswords(string input)
    {
        PasswordValidator.IsStrong(input).Should().BeTrue();
    }
}
