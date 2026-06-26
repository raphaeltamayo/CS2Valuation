using System.Windows;
using CStoValuation.App.Views;
using CStoValuation.Core.Abstractions;

namespace CStoValuation.App.Authentication;

/// <summary>
/// Shows the <see cref="SteamOpenIdLoginWindow"/> as a modal dialog and reports the result.
/// On success it also records the captured session cookies, which unlocks authenticated calls
/// such as the Market price-history endpoint.
/// </summary>
internal sealed class SteamOpenIdSignIn : ISteamSignIn
{
    private readonly ISteamSession _session;

    public SteamOpenIdSignIn(ISteamSession session) => _session = session;

    public Task<string?> SignInAsync()
    {
        var window = new SteamOpenIdLoginWindow { Owner = Application.Current.MainWindow };
        var succeeded = window.ShowDialog() == true;

        if (succeeded)
        {
            _session.SetCookies(window.CookieHeader);
        }

        return Task.FromResult(succeeded ? window.SteamId64 : null);
    }
}
