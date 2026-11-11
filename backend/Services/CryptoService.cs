using System.Security.Cryptography;
using System.Text;
using Ismedu.Api.Settings;

namespace Ismedu.Api.Services;

public interface ICryptoService
{
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
    string Fingerprint(string value);
}

public class CryptoService : ICryptoService
{
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _encryptionKey;
    private readonly byte[] _hashKey;

    public CryptoService(EncryptionSettings settings)
    {
        _encryptionKey = ReadKey(settings.EncryptionKey, "Encryption:EncryptionKey");
        _hashKey = ReadKey(settings.HashKey, "Encryption:HashKey");

        if (_encryptionKey.SequenceEqual(_hashKey))
        {
            throw new InvalidOperationException(
                "Encryption:EncryptionKey and Encryption:HashKey must be different keys.");
        }
    }

    public string Encrypt(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var encryptedBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Encrypt(nonce, plainBytes, encryptedBytes, tag);

        // Stored together as one text: nonce + tag + encrypted bytes
        var combined = new byte[NonceSize + TagSize + encryptedBytes.Length];
        nonce.CopyTo(combined, 0);
        tag.CopyTo(combined, NonceSize);
        encryptedBytes.CopyTo(combined, NonceSize + TagSize);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string encryptedText)
    {
        var combined = Convert.FromBase64String(encryptedText);
        if (combined.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("The encrypted value is too short to be valid.");
        }

        var nonce = combined.AsSpan(0, NonceSize);
        var tag = combined.AsSpan(NonceSize, TagSize);
        var encryptedBytes = combined.AsSpan(NonceSize + TagSize);
        var plainBytes = new byte[encryptedBytes.Length];

        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Decrypt(nonce, encryptedBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string Fingerprint(string value)
    {
        var hash = HMACSHA256.HashData(_hashKey, Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }

    private static byte[] ReadKey(string base64Key, string settingName)
    {
        try
        {
            var key = Convert.FromBase64String(base64Key);
            if (key.Length == KeySize)
            {
                return key;
            }
        }
        catch (FormatException)
        {}

        throw new InvalidOperationException(
            $"'{settingName}' must be a {KeySize}-byte key written as Base64. " +
            "Set it with dotnet user-secrets (see README).");
    }
}