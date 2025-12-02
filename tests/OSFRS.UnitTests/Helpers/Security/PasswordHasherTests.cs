using FluentAssertions;
using OSFRS.Backend.Helpers;

namespace OSFRS.UnitTests.Helpers;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    // ------------------------------------------------------------
    // HASH != PLAINTEXT
    // ------------------------------------------------------------
    [Fact]
    public void Hash_ShouldNotMatchPlaintext()
    {
        var password = "Secret123!";

        var hash = _hasher.Hash(password);

        hash.Should().NotBe(password);
        hash.Should().Contain("$2"); // BCrypt header, sanity check
    }

    // ------------------------------------------------------------
    // HASH MATCH VERIFICATION
    // ------------------------------------------------------------
    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var password = "StrongPass123$";

        var hash = _hasher.Hash(password);

        _hasher.Verify(password, hash).Should().BeTrue();
    }

    // ------------------------------------------------------------
    // SALTING WORKS — DIFFERENT HASHES, SAME PASSWORD
    // ------------------------------------------------------------
    [Fact]
    public void Hash_ShouldProduceDifferentValues_ForSamePassword()
    {
        var password = "SameButSalted123!";

        var hash1 = _hasher.Hash(password);
        var hash2 = _hasher.Hash(password);

        hash1.Should().NotBe(hash2); // Because BCrypt salts each hash differently
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        var hash = _hasher.Hash("PasswordA!");

        _hasher.Verify("PasswordB!", hash).Should().BeFalse();
    }
}
