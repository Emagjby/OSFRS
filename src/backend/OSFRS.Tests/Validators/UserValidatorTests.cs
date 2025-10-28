using OSFRS.Backend.Validators;

namespace OSFRS.Tests.Validators;

public class UserValidatorTests
{
    public static IEnumerable<object[]> NameTestData =>
        new List<object[]>
        {
            new object[] { null!, false },
            new object[] { "", false },
            new object[] { " ", false },
            new object[] { "John Doe", true },
            new object[] { "J".PadLeft(51, 'J'), false } // over 50 chars
        };

    [Theory]
    [MemberData(nameof(NameTestData))]
    public void ValidateName_ReturnsExpected(string? input, bool expected)
    {
        var result = UserValidator.ValidateName(input!);
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> UsernameTestData =>
        new List<object[]>
        {
            new object[] { null!, false },
            new object[] { " ", false },
            new object[] { "ab", false }, // too short
            new object[] { "a".PadLeft(21, 'a'), false }, // too long
            new object[] { "valid_user123", true },
            new object[] { "invalid user!", false }
        };

    [Theory]
    [MemberData(nameof(UsernameTestData))]
    public void ValidateUsername_ReturnsExpected(string? input, bool expected)
    {
        var result = UserValidator.ValidateUsername(input!);
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> EmailTestData =>
        new List<object[]>
        {
            new object[] { null!, false },
            new object[] { " ", false },
            new object[] { "plainaddress", false },
            new object[] { "missingat.com", false },
            new object[] { "user@example.com", true }
        };

    [Theory]
    [MemberData(nameof(EmailTestData))]
    public void ValidateEmail_ReturnsExpected(string? input, bool expected)
    {
        var result = UserValidator.ValidateEmail(input!);
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> PasswordTestData =>
        new List<object[]>
        {
            new object[] { null!, false },
            new object[] { "", false },
            new object[] { "short1", false }, // too short
            new object[] { "NoDigitsHere", false },
            new object[] { "Valid123", true },
            new object[] { "AnotherValid1", true }
        };
        
    [Theory]
    [MemberData(nameof(PasswordTestData))]
    public void ValidatePassword_ReturnsExpected(string? input, bool expected)
    {
        var result = UserValidator.ValidatePassword(input!);
        Assert.Equal(expected, result);
    }
}