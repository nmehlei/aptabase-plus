namespace Aptabase.Features.Authentication;

/// <summary>
/// Decides whether a new account may be created for an email address.
/// An empty allowlist means open registration (upstream behaviour).
/// Entries are either a full email (user@example.com) or a domain prefixed with @ (@example.com).
/// </summary>
public static class SignupPolicy
{
    public static bool IsAllowed(string email, IReadOnlyCollection<string> allowlist)
    {
        if (allowlist.Count == 0)
            return true;

        var normalized = email.Trim().ToLowerInvariant();
        var at = normalized.LastIndexOf('@');
        if (at <= 0 || at == normalized.Length - 1)
            return false;

        var domain = normalized[at..];
        return allowlist.Any(entry => entry == normalized || (entry.StartsWith('@') && entry == domain));
    }

    public static string[] Parse(string raw) => raw
        .Split([',', ';', ' ', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(e => e.ToLowerInvariant())
        .Distinct()
        .ToArray();
}
