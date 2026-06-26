using System.Net.Http;
using System.Text.RegularExpressions;

namespace CStoValuation.App.Authentication;

/// <summary>
/// The mechanics of Steam's OpenID 2.0 sign-in: building the authentication request URL,
/// pulling the SteamID64 out of the positive assertion, and verifying that assertion
/// directly with Steam. The flow proves identity only — it does not grant any access to a
/// private inventory (the public-inventory constraint still applies).
/// </summary>
internal static partial class SteamOpenId
{
    public const string LoginEndpoint = "https://steamcommunity.com/openid/login";

    // We never run a server at these URLs: the WebView2 host intercepts the redirect to
    // ReturnTo and reads the query directly, cancelling navigation before it loads.
    //
    // The host uses the reserved ".invalid" TLD (RFC 2606): it is guaranteed never to resolve,
    // so the assertion can never leak to a real server even if interception ever failed. Note
    // we deliberately do NOT use "localhost"/"127.0.0.1" — Steam's Akamai WAF returns an
    // "Access Denied" page for OpenID requests whose return URL is loopback.
    public const string Realm = "https://cs2valuator.invalid/";
    public const string ReturnTo = "https://cs2valuator.invalid/openid-return";

    private const string OpenIdNamespace = "http://specs.openid.net/auth/2.0";
    private const string IdentifierSelect = "http://specs.openid.net/auth/2.0/identifier_select";

    [GeneratedRegex(@"https://steamcommunity\.com/openid/id/(7656119\d{10})")]
    private static partial Regex ClaimedIdPattern();

    /// <summary>Builds the URL that starts the "checkid_setup" sign-in flow.</summary>
    public static string BuildLoginUrl()
    {
        var parameters = new Dictionary<string, string>
        {
            ["openid.ns"] = OpenIdNamespace,
            ["openid.mode"] = "checkid_setup",
            ["openid.return_to"] = ReturnTo,
            ["openid.realm"] = Realm,
            ["openid.identity"] = IdentifierSelect,
            ["openid.claimed_id"] = IdentifierSelect,
        };

        return $"{LoginEndpoint}?{Encode(parameters)}";
    }

    /// <summary>True if <paramref name="uri"/> is Steam's redirect back to our return URL.</summary>
    public static bool IsReturnUrl(Uri uri) =>
        uri.GetLeftPart(UriPartial.Path).StartsWith(ReturnTo, StringComparison.OrdinalIgnoreCase);

    /// <summary>Extracts the SteamID64 from the positive assertion, or null if absent.</summary>
    public static string? ExtractSteamId(Uri returnUri)
    {
        var match = ClaimedIdPattern().Match(Uri.UnescapeDataString(returnUri.Query));
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Confirms the assertion is genuine by replaying it to Steam with
    /// <c>openid.mode=check_authentication</c>; Steam answers <c>is_valid:true</c> only for
    /// an assertion it actually issued. This is what stops a forged return URL.
    /// </summary>
    public static async Task<bool> VerifyAsync(Uri returnUri, HttpClient httpClient, CancellationToken cancellationToken)
    {
        var parameters = ParseQuery(returnUri.Query);
        parameters["openid.mode"] = "check_authentication";

        using var content = new FormUrlEncodedContent(parameters);
        using var response = await httpClient.PostAsync(LoginEndpoint, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return body.Contains("is_valid:true", StringComparison.Ordinal);
    }

    private static string Encode(IEnumerable<KeyValuePair<string, string>> parameters) =>
        string.Join('&', parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = Uri.UnescapeDataString(pair[..separator]);
            var value = Uri.UnescapeDataString(pair[(separator + 1)..]);
            result[key] = value;
        }

        return result;
    }
}
