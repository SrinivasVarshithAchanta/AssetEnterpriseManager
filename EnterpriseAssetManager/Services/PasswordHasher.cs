using System.Security.Cryptography;

namespace EnterpriseAssetManager.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string storedHash);
}

// Simple, self contained PBKDF2 password hasher.
// Stored format: "iterations.saltBase64.hashBase64".
// This is easy to explain in an interview and avoids pulling in ASP.NET Identity.
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;   // 128 bit salt
    private const int KeySize = 32;    // 256 bit derived key
    private const int Iterations = 100_000;
    private const char Delimiter = '.';
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join(Delimiter,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        string[] parts = storedHash.Split(Delimiter);
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out int iterations))
            return false;

        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expectedHash = Convert.FromBase64String(parts[2]);

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expectedHash.Length);

        // Constant time comparison avoids leaking information through timing.
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
