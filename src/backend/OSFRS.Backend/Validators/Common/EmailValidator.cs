using System.Text.RegularExpressions;

namespace OSFRS.Backend.Validators.Common;

public static class EmailValidator
{
    private static readonly Regex _regex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static bool IsValid(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return _regex.IsMatch(email);
    }
}