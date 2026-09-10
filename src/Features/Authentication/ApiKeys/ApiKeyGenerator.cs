using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Aptabase.Features.Authentication.ApiKeys;

public static class ApiKeyGenerator
{
    public const string Prefix = "aptb_";
    public const int DisplayPrefixLength = 12;
    private const int SecretBytes = 32;

    public static (string PlainText, string Hash, string DisplayPrefix) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(SecretBytes);
        // Proper base64url (RFC 4648 §5): '-'/'_' alphabet, no padding.
        // Fixed-length for a fixed input size, so the key is regex-validatable.
        var secret = Base64Url.EncodeToString(bytes);
        var plainText = $"{Prefix}{secret}";
        var hash = Hash(plainText);
        var displayPrefix = plainText[..Math.Min(DisplayPrefixLength, plainText.Length)];
        return (plainText, hash, displayPrefix);
    }

    public static string Hash(string plainTextKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainTextKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
