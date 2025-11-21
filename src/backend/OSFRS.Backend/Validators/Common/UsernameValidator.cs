using System.Text.RegularExpressions;

namespace OSFRS.Backend.Validators.Common;

public static class UsernameValidator
{
    private static readonly Regex _regex = new(
        @"^[A-Za-z0-9][A-Za-z0-9_]{2,29}$",
        RegexOptions.Compiled
    );

    public static bool IsValid(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return _regex.IsMatch(username);
    }
}