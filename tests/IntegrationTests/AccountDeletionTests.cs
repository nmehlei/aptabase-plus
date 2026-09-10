using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Xunit;

namespace Aptabase.IntegrationTests;

[Collection("Integration Tests")]
public class AccountDeletionTests
{
    private readonly IntegrationTestsFixture _fixture;

    public AccountDeletionTests(IntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DeleteAccount_WithExistingApiKey_Returns200_NotFkViolation()
    {
        // Regression: api_keys.user_id FK used to block account deletion (23503 -> 500).
        var account = await _fixture.CreateFreshAccountAsync("Doomed User");
        await account.CreateApiKeyAsync("some key");

        var response = await account.DeleteAccountAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteAccount_WithAppsAndApiKey_Returns200()
    {
        var account = await _fixture.CreateFreshAccountAsync("Doomed User Two");
        var key = await account.CreateApiKeyAsync("tf");
        var client = account.AuthenticatedWith(key.Key);
        var create = await client.PostAsJsonAsync("/api/v0/apps", new { name = "Throwaway App" });
        create.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await account.DeleteAccountAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
