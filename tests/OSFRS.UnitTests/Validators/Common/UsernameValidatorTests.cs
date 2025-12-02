using FluentAssertions;
using OSFRS.Backend.Validators.Common;

namespace OSFRS.UnitTests.Validators.Common;

public class UsernameValidatorTests
{
    // ============================================================
    // NULL / EMPTY
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_ShouldReturnFalse_ForNullOrWhitespace(string? input)
    {
        UsernameValidator.IsValid(input).Should().BeFalse();
    }

    // ============================================================
    // TOO SHORT (< 3)
    // ============================================================

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void IsValid_ShouldReturnFalse_WhenTooShort(string input)
    {
        UsernameValidator.IsValid(input).Should().BeFalse();
    }

    // ============================================================
    // TOO LONG (> 30)
    // ============================================================

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenTooLong()
    {
        string longName = new string('a', 31);
        UsernameValidator.IsValid(longName).Should().BeFalse();
    }

    // ============================================================
    // INVALID CHARACTERS
    // ============================================================

    [Theory]
    [InlineData("abc!")]
    [InlineData("john.doe")]
    [InlineData("hello-world")]
    [InlineData("space man")]
    [InlineData("bad@name")]
    [InlineData("💀💀💀")]
    public void IsValid_ShouldReturnFalse_WhenContainsInvalidCharacters(string input)
    {
        UsernameValidator.IsValid(input).Should().BeFalse();
    }

    // ============================================================
    // MUST START WITH LETTER OR DIGIT
    // ============================================================

    [Theory]
    [InlineData("_test")]
    [InlineData("-bad")]
    [InlineData(".dotstart")]
    public void IsValid_ShouldReturnFalse_WhenStartsWithInvalidCharacter(string input)
    {
        UsernameValidator.IsValid(input).Should().BeFalse();
    }

    // ============================================================
    // VALID USERNAMES
    // ============================================================

    [Theory]
    [InlineData("abc")]
    [InlineData("john_doe")]
    [InlineData("User123")]
    [InlineData("a_b_c")]
    [InlineData("Z9_hello")]
    [InlineData("valid_username12345")]
    public void IsValid_ShouldReturnTrue_ForValidUsernames(string input)
    {
        UsernameValidator.IsValid(input).Should().BeTrue();
    }
}
