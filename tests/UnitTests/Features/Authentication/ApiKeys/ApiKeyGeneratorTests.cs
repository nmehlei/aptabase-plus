using Aptabase.Features.Authentication.ApiKeys;
using AwesomeAssertions;
using Xunit;

namespace Aptabase.UnitTests.Features.Authentication.ApiKeys;

public class ApiKeyGeneratorTests
{
    [Fact]
    public void Generate_ProducesUniqueKeysWithExpectedPrefix()
    {
        var (plainTextA, hashA, displayPrefixA) = ApiKeyGenerator.Generate();
        var (plainTextB, hashB, _) = ApiKeyGenerator.Generate();

        plainTextA.Should().StartWith(ApiKeyGenerator.Prefix);
        displayPrefixA.Should().Be(plainTextA[..12]);
        plainTextA.Should().NotBe(plainTextB);
        hashA.Should().NotBe(hashB);
    }

    [Fact]
    public void Hash_IsDeterministicSha256Hex()
    {
        var hash1 = ApiKeyGenerator.Hash("aptb_sample");
        var hash2 = ApiKeyGenerator.Hash("aptb_sample");

        hash1.Should().Be(hash2);
        hash1.Should().HaveLength(64);
        hash1.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Generate_ProducesFixedLengthBase64UrlKey()
    {
        var (plainTextA, _, _) = ApiKeyGenerator.Generate();
        var (plainTextB, _, _) = ApiKeyGenerator.Generate();

        // 32 random bytes -> 43 chars of unpadded base64url, plus the "aptb_" prefix.
        plainTextA.Should().HaveLength(ApiKeyGenerator.Prefix.Length + 43);
        plainTextA.Length.Should().Be(plainTextB.Length);

        var secret = plainTextA[ApiKeyGenerator.Prefix.Length..];
        secret.Should().MatchRegex("^[A-Za-z0-9_-]{43}$");
        secret.Should().NotContain("+").And.NotContain("/").And.NotContain("=");
    }

    [Fact]
    public void Generate_HashMatchesHashOfPlainText()
    {
        var (plainText, hash, _) = ApiKeyGenerator.Generate();

        ApiKeyGenerator.Hash(plainText).Should().Be(hash);
    }
}
