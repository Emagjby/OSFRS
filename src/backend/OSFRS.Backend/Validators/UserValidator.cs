using System.Text.RegularExpressions;

namespace OSFRS.Backend.Validators;

public static class UserValidator
{
    public static bool ValidateName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name.Length <= 50;

    public static bool ValidateUsername(string username) =>
        !string.IsNullOrWhiteSpace(username) &&
        Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,20}$");

    public static bool ValidateEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) &&
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static bool ValidatePassword(string password) =>
        !string.IsNullOrWhiteSpace(password) &&
        password.Length >= 8 &&
        Regex.IsMatch(password, @"\d");
}