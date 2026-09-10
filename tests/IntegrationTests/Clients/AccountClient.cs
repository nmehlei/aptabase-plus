using System.Net;
using AwesomeAssertions;
using Aptabase.Features.Apps;
using Aptabase.Features.Authentication;
using Aptabase.Features.Authentication.ApiKeys;
using Aptabase.Features.Stats;

namespace Aptabase.IntegrationTests.Clients;

public class AccountClient
{
    private readonly HttpClient _client;
    private readonly Func<HttpClient> _clientFactory;

    public AccountClient(HttpClient client, Func<HttpClient> clientFactory)
    {
        _client = client;
        _clientFactory = clientFactory;
    }

    public async Task CreateAccount(string name, string email)
    {
        var response = await _client.PostAsJsonAsync("/api/_auth/register", new { name, email });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var confirmUrl = await MailCatcher.GetLinkSentTo(email);
        response = await _client.GetAsync(confirmUrl);
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    public async Task<Application> CreateApp(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/_apps", new { name });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var apps = await _client.GetFromJsonAsync<Application[]>("/api/_apps");
        var app = apps?.FirstOrDefault(x => x.Name == name);
        return app ?? throw new Exception("No app found");
    }

    public async Task<HttpResponseMessage> GetKeyMetrics(string appId, string period)
    {
        return await _client.GetAsync($"/api/_stats/metrics?buildMode=release&period={period}&appId={appId}");
    }

    public async Task<long> CountEvents(string appId, string period)
    {
        var metrics = await _client.GetFromJsonAsync<KeyMetrics>($"/api/_stats/metrics?buildMode=release&period={period}&appId={appId}");
        return metrics?.Current.Events ?? 0;
    }

    public async Task<HttpResponseMessage> GetErrors(string appId)
    {
        return await _client.GetAsync($"/api/v0/apps/{appId}/errors?buildMode=release");
    }

    public async Task<HttpResponseMessage> GetErrorById(string appId, string errorId)
    {
        return await _client.GetAsync($"/api/v0/apps/{appId}/errors/{errorId}?buildMode=release");
    }

    public async Task<SessionTimeline?> GetSessionTimeline(string appId, object sessionId)
    {
        return await _client.GetFromJsonAsync<SessionTimeline>($"/api/_stats/live-session-details?buildMode=release&appId={appId}&sessionId={sessionId}");
    }

    public async Task<UserAccount?> GetMeAsync()
    {
        return await _client.GetFromJsonAsync<UserAccount>("/api/_auth/me");
    }

    public async Task<ApiKeyCreated> CreateApiKeyAsync(string name, DateTimeOffset? expiresAt = null)
    {
        var response = await _client.PostAsJsonAsync("/api/v0/api-keys", new { name, expiresAt });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ApiKeyCreated>())!;
    }

    public async Task<ApiKeySummary[]> ListApiKeysAsync()
    {
        return (await _client.GetFromJsonAsync<ApiKeySummary[]>("/api/v0/api-keys"))!;
    }

    public async Task<HttpResponseMessage> DeleteApiKeyAsync(string keyId)
    {
        return await _client.DeleteAsync($"/api/v0/api-keys/{keyId}");
    }

    public async Task<HttpResponseMessage> DeleteAccountAsync()
    {
        return await _client.PostAsync("/api/_auth/account/delete", null);
    }

    /// <summary>The raw shared cookie-session client (no Authorization header).</summary>
    public HttpClient CookieClient => _client;

    /// <summary>
    /// Returns a FRESH client authenticated with the given bearer key. Must not mutate
    /// the shared cookie-session client, which is reused across the whole test collection.
    /// </summary>
    public HttpClient AuthenticatedWith(string apiKey)
    {
        var client = _clientFactory();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }
}