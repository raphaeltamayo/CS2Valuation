using System.Net.Http;
using System.Text.RegularExpressions;

namespace CStoValuation.App.Authentication;

internal static partial class SteamOpenId
{
    public const string LoginEndpoint = "https://steamcommunity.com/openid/login";

    public const string Realm = "https://cs2valuator.app/";
    public const string ReturnTo = "https://cs2valuator.app/signin";

    private const string OpenIdNamespace = "http://specs.openid.net/auth/2.0";
    private const string IdentifierSelect = "http://specs.openid.net/auth/2.0/identifier_select";

    [GeneratedRegex(@"https://steamcommunity\.com/openid/id/(7656119\d{10})")]
    private static partial Regex ClaimedIdPattern();

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

    public static bool IsReturnUrl(Uri uri) =>
        uri.GetLeftPart(UriPartial.Path).StartsWith(ReturnTo, StringComparison.OrdinalIgnoreCase);

    public static string? ExtractSteamId(Uri returnUri)
    {
        var match = ClaimedIdPattern().Match(Uri.UnescapeDataString(returnUri.Query));
        return match.Success ? match.Groups[1].Value : null;
    }

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
