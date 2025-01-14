using System.Security.Cryptography;

namespace Auth;

internal sealed class PasswordHasher
{
    private const int SALT_SIZE = 16;
    private const int HASH_SIZE = 32;
    private const int ITERATIONS = 100000;
    private readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    internal string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, ITERATIONS, Algorithm, HASH_SIZE);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    internal bool Verify(string password, string passwordHash)
    {
        string[] parts = passwordHash.Split("-");

        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            ITERATIONS,
            Algorithm,
            HASH_SIZE
        );

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}
