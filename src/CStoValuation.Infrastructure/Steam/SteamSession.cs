using CStoValuation.Core.Abstractions;

namespace CStoValuation.Infrastructure.Steam;

/// <summary>
/// In-memory holder for the current Steam web session cookies. Registered as a singleton so the
/// cookies captured by the sign-in window are visible to the history service. Kept in memory only
/// (never persisted) since it carries a sensitive session token.
/// </summary>
public sealed class SteamSession : ISteamSession
{
    private volatile string? _cookieHeader;

    public string? CookieHeader => _cookieHeader;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_cookieHeader);

    public void SetCookies(string? cookieHeader) =>
        _cookieHeader = string.IsNullOrWhiteSpace(cookieHeader) ? null : cookieHeader;
}
