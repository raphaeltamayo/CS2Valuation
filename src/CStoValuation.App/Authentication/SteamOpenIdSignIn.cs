using System.Windows;
using CStoValuation.App.Views;

namespace CStoValuation.App.Authentication;

/// <summary>
/// Shows the <see cref="SteamOpenIdLoginWindow"/> as a modal dialog and reports the result.
/// This is the one place that bridges the abstract sign-in gesture to a concrete window.
/// </summary>
internal sealed class SteamOpenIdSignIn : ISteamSignIn
{
    public Task<string?> SignInAsync()
    {
        var window = new SteamOpenIdLoginWindow { Owner = Application.Current.MainWindow };
        var succeeded = window.ShowDialog() == true;
        return Task.FromResult(succeeded ? window.SteamId64 : null);
    }
}
