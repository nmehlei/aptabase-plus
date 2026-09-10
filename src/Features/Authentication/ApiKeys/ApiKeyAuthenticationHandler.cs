using System.Security.Claims;
using System.Text.Encodings.Web;
using Aptabase.Data;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Aptabase.Features.Authentication.ApiKeys;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";

    private readonly IDbContext _db;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IDbContext db)
        : base(options, logger, encoder)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var rawKey = header["Bearer ".Length..].Trim();
        if (!rawKey.StartsWith(ApiKeyGenerator.Prefix, StringComparison.Ordinal))
            return AuthenticateResult.Fail("Malformed API key");

        var hash = ApiKeyGenerator.Hash(rawKey);

        var row = await _db.Connection.QueryFirstOrDefaultAsync<ApiKeyAuthRow>(
            @"SELECT k.id as key_id, u.id, u.name, u.email
              FROM api_keys k
              INNER JOIN users u ON u.id = k.user_id
              WHERE k.key_hash = @hash
              AND (k.expires_at IS NULL OR k.expires_at > now())",
            new { hash });

        if (row is null)
            return AuthenticateResult.Fail("Invalid, revoked, or expired API key");

        UpdateLastUsedBestEffort(row.KeyId);

        var claims = new[]
        {
            new Claim("id", row.Id),
            new Claim("name", row.Name),
            new Claim("email", row.Email),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return AuthenticateResult.Success(ticket);
    }

    private void UpdateLastUsedBestEffort(string keyId)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _db.Connection.ExecuteAsync(
                    "UPDATE api_keys SET last_used_at = now() WHERE id = @id",
                    new { id = keyId });
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to update api_keys.last_used_at for {KeyId}", keyId);
            }
        });
    }
}

internal class ApiKeyAuthRow
{
    public string KeyId { get; set; } = "";
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}
