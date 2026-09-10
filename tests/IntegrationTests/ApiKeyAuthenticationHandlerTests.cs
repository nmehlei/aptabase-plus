using System.Net;
using System.Net.Http.Headers;
using Aptabase.Data;
using Aptabase.Features.Authentication.ApiKeys;
using AwesomeAssertions;
using Dapper;
using Xunit;

namespace Aptabase.IntegrationTests;

[Collection("Integration Tests")]
public class ApiKeyAuthenticationHandlerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public ApiKeyAuthenticationHandlerTests(IntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ValidApiKey_AuthenticatesAsKeyOwner()
    {
        var me = await _fixture.UserA.GetMeAsync();
        var plainTextKey = await InsertApiKeyForUserAsync(me!.Id);

        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", plainTextKey);

        var response = await client.GetAsync("/api/_auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Aptabase.Features.Authentication.UserAccount>();
        body!.Id.Should().Be(me.Id);
    }

    [Fact]
    public async Task UnknownApiKey_ReturnsUnauthorized()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "aptb_doesnotexist");

        var response = await client.GetAsync("/api/_auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExpiredApiKey_ReturnsUnauthorized()
    {
        var me = await _fixture.UserA.GetMeAsync();
        var plainTextKey = await InsertApiKeyForUserAsync(me!.Id, expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", plainTextKey);

        var response = await client.GetAsync("/api/_auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<string> InsertApiKeyForUserAsync(string userId, DateTimeOffset? expiresAt = null)
    {
        var db = _fixture.GetService<IDbContext>();
        var (plainText, hash, displayPrefix) = ApiKeyGenerator.Generate();
        var id = NanoId.New();

        await db.Connection.ExecuteAsync(
            @"INSERT INTO api_keys (id, user_id, name, key_hash, key_prefix, expires_at)
              VALUES (@id, @userId, 'test key', @hash, @displayPrefix, @expiresAt)",
            new { id, userId, hash, displayPrefix, expiresAt });

        return plainText;
    }
}
