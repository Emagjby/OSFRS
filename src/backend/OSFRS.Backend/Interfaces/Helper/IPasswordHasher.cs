namespace OSFRS.Backend.Interfaces.Helper;

/// <summary>
/// Provides password hashing and verification functionality.
/// </summary>
/// <remarks>
/// Implementations of this interface must use a secure, modern hashing algorithm
/// (e.g., BCrypt, Argon2, PBKDF2).  
/// This abstraction allows the hashing strategy to be swapped or upgraded without
/// modifying authentication logic.
/// </remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a secure hash for the specified plaintext password.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>A hashed representation of the password.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies that a plaintext password matches the stored hashed password.
    /// </summary>
    /// <param name="password">The plaintext password provided by the user.</param>
    /// <param name="hashedPassword">The previously stored hashed password.</param>
    /// <returns>
    /// <c>true</c> if the password is valid and matches the hash; otherwise <c>false</c>.
    /// </returns>
    bool Verify(string password, string hashedPassword);
}