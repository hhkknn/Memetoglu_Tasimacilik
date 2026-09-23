using System.Security.Cryptography;

namespace MemetogluWeb.Services;

/// <summary>
/// Şifre özetleme.
///
/// PBKDF2-SHA256 kullanır; .NET'in kendi kriptografi kütüphanesiyle gelir,
/// harici paket gerektirmez. Düz şifre hiçbir yerde saklanmaz.
/// </summary>
public static class PasswordHasher
{
    private const int Iterations = 210_000; // OWASP'ın PBKDF2-SHA256 önerisi
    private const int SaltSize = 16;
    private const int HashSize = 32;

    /// <summary>Şifreyi "iterasyon.tuz.ozet" biçiminde özetler.</summary>
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Şifreyi doğrular. Karşılaştırma sabit zamanlıdır: şifrenin ne kadarının
    /// doğru olduğu geçen süreden anlaşılamaz.
    /// </summary>
    public static bool Verify(string password, string stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return false;
        }

        var parcalar = stored.Split('.', 3);
        if (parcalar.Length != 3
            || !int.TryParse(parcalar[0], out var iterasyon)
            || iterasyon <= 0)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parcalar[1]);
            var beklenen = Convert.FromBase64String(parcalar[2]);

            var hesaplanan = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iterasyon, HashAlgorithmName.SHA256, beklenen.Length);

            return CryptographicOperations.FixedTimeEquals(hesaplanan, beklenen);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
