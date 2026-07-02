using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AlAtaaClinic.Desktop.Services;

public sealed class LicenseService
{
    private const string LicenseFile = "license.dat";
    private static readonly byte[] LicensingKey = SHA256.HashData(
        Encoding.UTF8.GetBytes("AlAtaaClinic-Licensing-v1"));

    public LicenseInfo CurrentLicense { get; private set; } = new();

    public void LoadLicense()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, LicenseFile);
            if (!File.Exists(path))
            {
                CurrentLicense = LicenseInfo.Trial();
                return;
            }

            var protectedBytes = File.ReadAllBytes(path);
            var json = UnprotectLocal(protectedBytes);
            CurrentLicense = LicenseInfo.FromJson(json);
        }
        catch
        {
            CurrentLicense = LicenseInfo.Trial();
        }
    }

    public bool Activate(string key)
    {
        try
        {
            var json = DecryptActivationKey(key);
            var license = LicenseInfo.FromJson(json);

            if (license.ExpiryDate < DateTime.UtcNow)
                return false;

            if (string.IsNullOrWhiteSpace(license.ClinicName))
                return false;

            if (license.MaxUsers < 1)
                return false;

            CurrentLicense = license;
            SaveLicense();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateKey(string clinicName, DateTime expiryDate, int maxUsers)
    {
        var license = new LicenseInfo
        {
            ClinicName = clinicName,
            ExpiryDate = expiryDate,
            MaxUsers = maxUsers,
            LicenseId = Guid.NewGuid().ToString("N")[..12].ToUpper(),
            ActivatedAt = DateTime.UtcNow
        };

        return EncryptActivationKey(license.ToJson());
    }

    public bool IsValid => CurrentLicense.IsActive && CurrentLicense.ExpiryDate > DateTime.UtcNow;
    public bool IsExpired => CurrentLicense.ExpiryDate <= DateTime.UtcNow;
    public int DaysRemaining => Math.Max(0, (CurrentLicense.ExpiryDate - DateTime.UtcNow).Days);

    private void SaveLicense()
    {
        var path = Path.Combine(AppContext.BaseDirectory, LicenseFile);
        var json = CurrentLicense.ToJson();
        var protectedBytes = ProtectLocal(json);
        File.WriteAllBytes(path, protectedBytes);
    }

    private static byte[] ProtectLocal(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        return ProtectedData.Protect(plainBytes, optionalEntropy: null, DataProtectionScope.LocalMachine);
    }

    private static string UnprotectLocal(byte[] protectedBytes)
    {
        var plainBytes = ProtectedData.Unprotect(protectedBytes, optionalEntropy: null, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private static string EncryptActivationKey(string plainText)
    {
        var iv = new byte[16];
        RandomNumberGenerator.Fill(iv);

        using var aes = Aes.Create();
        aes.Key = LicensingKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[iv.Length + encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    private static string DecryptActivationKey(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - 16];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
        Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

        using var aes = Aes.Create();
        aes.Key = LicensingKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}

public sealed class LicenseInfo
{
    public string LicenseId { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int MaxUsers { get; set; } = 5;
    public DateTime ActivatedAt { get; set; }

    public bool IsActive => !string.IsNullOrWhiteSpace(ClinicName);

    public static LicenseInfo Trial() => new()
    {
        ClinicName = "Trial",
        ExpiryDate = DateTime.UtcNow.AddDays(14),
        MaxUsers = 2,
        LicenseId = "TRIAL"
    };

    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this);

    public static LicenseInfo FromJson(string json) =>
        System.Text.Json.JsonSerializer.Deserialize<LicenseInfo>(json) ?? new();
}
