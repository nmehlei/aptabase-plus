using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Xunit;

namespace Aptabase.IntegrationTests;

[Collection("Integration Tests")]
public class AppsV0Tests
{
    private readonly IntegrationTestsFixture _fixture;

    public AppsV0Tests(IntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FullAppLifecycle_ViaApiKey()
    {
        var key = await _fixture.UserA.CreateApiKeyAsync("terraform");
        var client = _fixture.UserA.AuthenticatedWith(key.Key);

        var createResponse = await client.PostAsJsonAsync("/api/v0/apps", new { name = "My App" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = (await createResponse.Content.ReadFromJsonAsync<AppV0>())!;
        created.Name.Should().Be("My App");
        created.AppKey.Should().NotBeNullOrEmpty();

        var getResponse = await client.GetAsync($"/api/v0/apps/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await client.PutAsJsonAsync($"/api/v0/apps/{created.Id}", new { name = "Renamed App", icon = "" });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<AppV0>())!;
        updated.Name.Should().Be("Renamed App");

        var shareResponse = await client.PutAsync($"/api/v0/apps/{created.Id}/shares/friend@example.com", null);
        shareResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var sharesResponse = await client.GetFromJsonAsync<ShareV0[]>($"/api/v0/apps/{created.Id}/shares");
        sharesResponse.Should().ContainSingle(s => s.Email == "friend@example.com");

        var unshareResponse = await client.DeleteAsync($"/api/v0/apps/{created.Id}/shares/friend@example.com");
        unshareResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await client.DeleteAsync($"/api/v0/apps/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        getResponse = await client.GetAsync($"/api/v0/apps/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetApp_OwnedByAnotherUser_ReturnsNotFound()
    {
        var keyA = await _fixture.UserA.CreateApiKeyAsync("k1");
        var clientA = _fixture.UserA.AuthenticatedWith(keyA.Key);
        var createResponse = await clientA.PostAsJsonAsync("/api/v0/apps", new { name = "Private App" });
        var created = (await createResponse.Content.ReadFromJsonAsync<AppV0>())!;

        var keyB = await _fixture.UserB.CreateApiKeyAsync("k2");
        var clientB = _fixture.UserB.AuthenticatedWith(keyB.Key);
        var response = await clientB.GetAsync($"/api/v0/apps/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record AppV0(string Id, string Name, string AppKey);
    private record ShareV0(string Email);
}
