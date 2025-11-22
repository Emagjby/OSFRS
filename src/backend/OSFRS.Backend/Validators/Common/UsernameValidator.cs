using System.Text.RegularExpressions;

namespace OSFRS.Backend.Validators.Common;

/// <summary>
/// Provides validation utilities for checking username format and structural rules.
/// </summary>
public static class UsernameValidator
{
    // Pattern:
    //  - Must start with a letter or digit
    //  - May contain letters, digits, or underscores
    //  - Length between 3 and 30 characters
    private static readonly Regex _regex = new(
        @"^[A-Za-z0-9][A-Za-z0-9_]{2,29}$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Determines whether the provided username meets the required format.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <returns>
    /// <c>true</c> if the username is non-empty and matches the defined character and length rules;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValid(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return _regex.IsMatch(username);
    }
}