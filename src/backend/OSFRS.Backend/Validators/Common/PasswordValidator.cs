using System.Text.RegularExpressions;

namespace OSFRS.Backend.Validators.Common;

public static class PasswordValidator
{
    private static readonly Regex _regex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        RegexOptions.Compiled
    );

    public static bool IsStrong(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return _regex.IsMatch(password);
    }
}