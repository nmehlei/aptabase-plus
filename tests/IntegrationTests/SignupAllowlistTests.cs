using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Aptabase.IntegrationTests.Clients;
using Xunit;

namespace Aptabase.IntegrationTests;

[Collection("Integration Tests")]
public class SignupAllowlistTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SignupAllowlistTests(IntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Register_WithAllowedDomain_SendsMagicLink()
    {
        var email = $"allowed.{Guid.NewGuid()}@example.com";

        var response = await _fixture.CreateClient().PostAsJsonAsync("/api/_auth/register", new { name = "Allowed User", email });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await MailCatcher.GetLinkSentTo(email)).Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithBlockedAddress_Returns200ButSendsNoEmail()
    {
        var email = $"blocked.{Guid.NewGuid()}@not-allowed.org";

        var response = await _fixture.CreateClient().PostAsJsonAsync("/api/_auth/register", new { name = "Blocked User", email });

        // Same response as a successful sign-up so the allowlist can't be probed.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await FluentActions.Awaiting(() => MailCatcher.GetLinkSentTo(email))
            .Should().ThrowAsync<Exception>().WithMessage("No message found*");
    }
}
