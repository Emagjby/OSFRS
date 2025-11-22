using System.Text.RegularExpressions;

namespace OSFRS.Backend.Validators.Common;

/// <summary>
/// Provides password strength validation based on character composition rules.
/// </summary>
public static class PasswordValidator
{
    // Pattern:
    //  - At least one lowercase letter
    //  - At least one uppercase letter
    //  - At least one digit
    //  - At least one symbol
    //  - Minimum length: 8 characters
    private static readonly Regex _regex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Determines whether the provided password meets strength requirements.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>
    /// <c>true</c> if the password is non-empty and matches all strength criteria;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool IsStrong(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return _regex.IsMatch(password);
    }
}