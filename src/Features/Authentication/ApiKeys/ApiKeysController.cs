using System.ComponentModel.DataAnnotations;
using Aptabase.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace Aptabase.Features.Authentication.ApiKeys;

public class CreateApiKeyRequestBody
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    public DateTimeOffset? ExpiresAt { get; set; }
}

public class ApiKeySummary
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string KeyPrefix { get; set; } = "";
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ApiKeyCreated : ApiKeySummary
{
    public string Key { get; set; } = "";
}

[ApiController, IsAuthenticated]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ApiKeysController : Controller
{
    private readonly IDbContext _db;

    public ApiKeysController(IDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    [HttpGet("/api/v0/api-keys")]
    public async Task<IActionResult> List()
    {
        var user = this.GetCurrentUserIdentity();
        var keys = await _db.Connection.QueryAsync<ApiKeySummary>(
            @"SELECT id, name, key_prefix, last_used_at, expires_at, created_at
              FROM api_keys
              WHERE user_id = @userId
              ORDER BY created_at DESC",
            new { userId = user.Id });

        return Ok(keys);
    }

    [HttpPost("/api/v0/api-keys")]
    public async Task<IActionResult> Create([FromBody] CreateApiKeyRequestBody body)
    {
        var user = this.GetCurrentUserIdentity();
        var (plainText, hash, displayPrefix) = ApiKeyGenerator.Generate();
        var id = NanoId.New();
        var createdAt = DateTimeOffset.UtcNow;

        await _db.Connection.ExecuteAsync(
            @"INSERT INTO api_keys (id, user_id, name, key_hash, key_prefix, expires_at)
              VALUES (@id, @userId, @name, @hash, @displayPrefix, @expiresAt)",
            new { id, userId = user.Id, name = body.Name, hash, displayPrefix, expiresAt = body.ExpiresAt });

        return Ok(new ApiKeyCreated
        {
            Id = id,
            Name = body.Name,
            KeyPrefix = displayPrefix,
            ExpiresAt = body.ExpiresAt,
            CreatedAt = createdAt,
            Key = plainText,
        });
    }

    [HttpDelete("/api/v0/api-keys/{keyId}")]
    public async Task<IActionResult> Delete(string keyId)
    {
        var user = this.GetCurrentUserIdentity();
        var affected = await _db.Connection.ExecuteAsync(
            "DELETE FROM api_keys WHERE id = @keyId AND user_id = @userId",
            new { keyId, userId = user.Id });

        if (affected == 0)
            return NotFound();

        return Ok(new { });
    }
}
