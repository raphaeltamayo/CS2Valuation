namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Holds the authenticated Steam web session (the cookies harvested from the WebView2 sign-in)
/// for the current run. Endpoints like the Market price-history API only answer logged-in
/// requests, so services consult this to attach the session when one is available.
/// </summary>
public interface ISteamSession
{
    /// <summary>The <c>Cookie</c> header value for steamcommunity.com, or null if not signed in.</summary>
    string? CookieHeader { get; }

    /// <summary>True once a session has been captured.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Records (or clears, when null/blank) the session cookies.</summary>
    void SetCookies(string? cookieHeader);
}
