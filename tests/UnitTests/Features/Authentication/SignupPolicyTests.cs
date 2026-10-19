using Aptabase.Features.Authentication;
using AwesomeAssertions;
using Xunit;

namespace Aptabase.UnitTests.Features.Authentication;

public class SignupPolicyTests
{
    [Fact]
    public void Empty_allowlist_allows_everyone() =>
        SignupPolicy.IsAllowed("a@b.com", []).Should().BeTrue();

    [Theory]
    [InlineData("me@example.com", true)]
    [InlineData("ME@Example.com ", true)]
    [InlineData("other@example.com", false)]
    [InlineData("colleague@corp.io", true)]
    [InlineData("x@evilcorp.io", false)]
    [InlineData("x@sub.corp.io", false)]
    [InlineData("me@example.com@evil.com", false)]
    [InlineData("not-an-email", false)]
    public void Matches_emails_and_domains(string email, bool expected) =>
        SignupPolicy.IsAllowed(email, SignupPolicy.Parse("me@example.com, @corp.io")).Should().Be(expected);

    [Fact]
    public void Parse_ignores_blanks_and_case() =>
        SignupPolicy.Parse(" A@B.com,, @C.io ;").Should().Equal("a@b.com", "@c.io");
}
