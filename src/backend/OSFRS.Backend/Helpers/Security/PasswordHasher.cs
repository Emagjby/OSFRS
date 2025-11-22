using OSFRS.Backend.Interfaces.Helper;

namespace OSFRS.Backend.Helpers;

/// <summary>
/// Provides password hashing and verification functionality using BCrypt.
/// </summary>
/// <remarks>
/// BCrypt automatically handles salting and safe hashing cost factors,
/// ensuring strong resistance against brute-force attacks.
/// </remarks>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Generates a secure BCrypt hash for the provided plaintext password.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>A hashed and salted password string.</returns>
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies that a plaintext password matches its BCrypt hashed version.
    /// </summary>
    /// <param name="password">The plaintext password to check.</param>
    /// <param name="hashedPassword">The stored hashed password.</param>
    /// <returns>
    /// <c>true</c> if the password matches; otherwise, <c>false</c>.
    /// </returns>
    public bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}