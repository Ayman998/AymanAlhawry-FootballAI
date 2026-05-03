using FootballAI.Application.Interfaces.AuthInterfaces;
using System.Security.Cryptography;

namespace FootballAI.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;          // 256 bits
    private const int HashSize = 32;          // 256 bits
    private const int Iterations = 100_000;   // OWASP recommendation
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;


    public (string Hash, string Salt) HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        // Generate a cryptographically secure random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Derive hash using PBKDF2
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(hash) ||
                    string.IsNullOrWhiteSpace(salt))
            return false;

        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var expectedHashBytes = Convert.FromBase64String(hash);

            var actualHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password, saltBytes, Iterations, Algorithm, HashSize);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
        }
        catch
        {
            return false;
        }
    }
}
