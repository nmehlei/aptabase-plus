using System.Net;
using System.Net.Http.Headers;
using AwesomeAssertions;
using Xunit;

namespace Aptabase.IntegrationTests;

[Collection("Integration Tests")]
public class ApiKeysTests
{
    private readonly IntegrationTestsFixture _fixture;

    public ApiKeysTests(IntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateListAndUseApiKey_FullLifecycle()
    {
        var created = await _fixture.UserA.CreateApiKeyAsync("ci key");
        created.Key.Should().StartWith("aptb_");
        created.KeyPrefix.Should().Be(created.Key[..12]);

        var list = await _fixture.UserA.ListApiKeysAsync();
        list.Should().ContainSingle(k => k.Id == created.Id);
        list.Should().OnlyContain(k => k.KeyPrefix.Length == 12);

        var keyClient = _fixture.CreateClient();
        keyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", created.Key);
        var meResponse = await keyClient.GetAsync("/api/_auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _fixture.UserA.DeleteApiKeyAsync(created.Id);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        meResponse = await keyClient.GetAsync("/api/_auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteApiKey_OwnedByAnotherUser_ReturnsNotFound()
    {
        var created = await _fixture.UserA.CreateApiKeyAsync("user a key");

        var response = await _fixture.UserB.DeleteApiKeyAsync(created.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var stillListed = await _fixture.UserA.ListApiKeysAsync();
        stillListed.Should().Contain(k => k.Id == created.Id);
    }
}
