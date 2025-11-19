namespace OSFRS.Backend.Interfaces.Helper;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}