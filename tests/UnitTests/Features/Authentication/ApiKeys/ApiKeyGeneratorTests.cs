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
    public void Generate_HashMatchesHashOfPlainText()
    {
        var (plainText, hash, _) = ApiKeyGenerator.Generate();

        ApiKeyGenerator.Hash(plainText).Should().Be(hash);
    }
}
