using Aptabase.Data;
using Aptabase.Features.Authentication;
using Aptabase.Features.Blob;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Aptabase.Features.Apps;

public class CreateAppV0RequestBody
{
    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string Name { get; set; } = "";
}

public class UpdateAppV0RequestBody
{
    public string Icon { get; set; } = "";

    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string Name { get; set; } = "";
}

[ApiController, IsAuthenticated]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AppsV0Controller : Controller
{
    private readonly IDbContext _db;
    private readonly EnvSettings _env;
    private readonly IBlobService _blobService;

    public AppsV0Controller(IDbContext db, EnvSettings env, IBlobService blobService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
    }

    [HttpGet("/api/v0/apps")]
    public async Task<IActionResult> ListApps()
    {
        var user = this.GetCurrentUserIdentity();
        var apps = await _db.Connection.QueryAsync<Application>(
            @"SELECT a.id, a.name, a.icon_path, a.app_key,
                     a.owner_id = @userId AS has_ownership, a.has_events
              FROM apps a
              LEFT JOIN app_shares s ON s.app_id = a.id
              WHERE (a.owner_id = @userId OR s.email = @userEmail)
              AND a.deleted_at IS NULL
              GROUP BY a.id, a.name, a.icon_path, a.app_key, a.owner_id
              ORDER BY a.name",
            new { userId = user.Id, userEmail = user.Email });

        return Ok(apps);
    }

    [HttpPost("/api/v0/apps")]
    public async Task<IActionResult> Create([FromBody] CreateAppV0RequestBody body)
    {
        var user = this.GetCurrentUserIdentity();
        var app = new Application
        {
            Id = NanoId.New(),
            Name = body.Name,
            AppKey = $"A-{_env.Region}-{NanoId.Numbers(10)}"
        };

        await _db.Connection.ExecuteAsync(
            @"INSERT INTO apps (id, owner_id, name, app_key, has_events)
              VALUES (@appId, @ownerId, @name, @appKey, false)",
            new { appId = app.Id, ownerId = user.Id, name = app.Name, appKey = app.AppKey });

        return Ok(app);
    }

    [HttpGet("/api/v0/apps/{appId}")]
    public async Task<IActionResult> GetById(string appId)
    {
        var app = await GetOwnedOrSharedApp(appId);
        if (app == null)
            return NotFound();

        return Ok(app);
    }

    [HttpPut("/api/v0/apps/{appId}")]
    public async Task<IActionResult> Update(string appId, [FromBody] UpdateAppV0RequestBody body, CancellationToken cancellationToken)
    {
        var app = await GetOwnedApp(appId);
        if (app == null)
            return NotFound();

        if (!string.IsNullOrEmpty(body.Icon))
        {
            var content = Convert.FromBase64String(body.Icon);
            app.IconPath = await _blobService.UploadAsync("icons", content, "image/png", cancellationToken);
        }

        app.Name = body.Name;
        await _db.Connection.ExecuteAsync(
            "UPDATE apps SET name = @name, icon_path = @iconPath WHERE id = @appId",
            new { appId = app.Id, name = app.Name, iconPath = app.IconPath });

        return Ok(app);
    }

    [HttpDelete("/api/v0/apps/{appId}")]
    public async Task<IActionResult> Delete(string appId)
    {
        var app = await GetOwnedApp(appId);
        if (app == null)
            return NotFound();

        await _db.Connection.ExecuteAsync(
            "UPDATE apps SET deleted_at = now() WHERE id = @appId",
            new { appId = app.Id });

        return Ok(new { });
    }

    [HttpGet("/api/v0/apps/{appId}/shares")]
    public async Task<IActionResult> ListShares(string appId)
    {
        var app = await GetOwnedApp(appId);
        if (app == null)
            return NotFound();

        var shares = await _db.Connection.QueryAsync<ApplicationShare>(
            "SELECT email, created_at FROM app_shares WHERE app_id = @appId",
            new { appId });

        return Ok(shares);
    }

    [HttpPut("/api/v0/apps/{appId}/shares/{email}")]
    public async Task<IActionResult> AddShare(string appId, string email)
    {
        var app = await GetOwnedApp(appId);
        if (app == null)
            return NotFound();

        await _db.Connection.ExecuteAsync(
            @"INSERT INTO app_shares (app_id, email)
              VALUES (@appId, @email)
              ON CONFLICT DO NOTHING",
            new { appId, email = email.ToLower() });

        return Ok(new { });
    }

    [HttpDelete("/api/v0/apps/{appId}/shares/{email}")]
    public async Task<IActionResult> RemoveShare(string appId, string email)
    {
        var app = await GetOwnedApp(appId);
        if (app == null)
            return NotFound();

        await _db.Connection.ExecuteAsync(
            "DELETE FROM app_shares WHERE app_id = @appId AND email = @email",
            new { appId, email = email.ToLower() });

        return Ok(new { });
    }

    private async Task<Application?> GetOwnedApp(string appId)
    {
        var user = this.GetCurrentUserIdentity();
        return await _db.Connection.QueryFirstOrDefaultAsync<Application>(
            @"SELECT id, name, icon_path, app_key, true as has_ownership, has_events
              FROM apps
              WHERE id = @appId AND owner_id = @userId AND deleted_at IS NULL",
            new { appId, userId = user.Id });
    }

    private async Task<Application?> GetOwnedOrSharedApp(string appId)
    {
        var user = this.GetCurrentUserIdentity();
        return await _db.Connection.QueryFirstOrDefaultAsync<Application>(
            @"SELECT a.id, a.name, a.icon_path, a.app_key,
                     a.owner_id = @userId as has_ownership, a.has_events
              FROM apps a
              LEFT JOIN app_shares s ON s.app_id = a.id
              WHERE a.id = @appId
              AND (a.owner_id = @userId OR s.email = @userEmail)
              AND a.deleted_at IS NULL
              GROUP BY a.id, a.name, a.icon_path, a.app_key, a.owner_id",
            new { appId, userId = user.Id, userEmail = user.Email });
    }
}
