using System.Security.Cryptography;
using System.Text;

namespace Grahplet.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16; // 128-bit
    private const int KeySize = 32;  // 256-bit
    private const int DefaultIterations = 100_000;
    private const string Version = "v1";

    public static string Hash(string password, int iterations = DefaultIterations)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return string.Join('$', Version, iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public static bool Verify(string storedHash, string password)
    {
        if (string.IsNullOrWhiteSpace(storedHash)) return false;
        var parts = storedHash.Split('$');
        if (parts.Length != 4) return false;
        // parts[0] = version
        if (parts[0] != Version) return false; // unsupported version
        if (!int.TryParse(parts[1], out var iterations)) return false;

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
