using System.Text.RegularExpressions;

namespace OSFRS.Backend.Validators.Common;

/// <summary>
/// Provides email validation using a compiled regular expression.
/// </summary>
public static class EmailValidator
{
    private static readonly Regex _regex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Determines whether a given email string matches a valid email pattern.
    /// </summary>
    /// <param name="email">Email string to validate.</param>
    /// <returns>
    /// <c>true</c> if the email is non-empty and matches the expected format; 
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool IsValid(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return _regex.IsMatch(email);
    }
}