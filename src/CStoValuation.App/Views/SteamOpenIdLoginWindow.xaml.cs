using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using CStoValuation.App.Authentication;
using Microsoft.Web.WebView2.Core;

namespace CStoValuation.App.Views;

/// <summary>
/// Hosts Steam's OpenID sign-in page in an embedded browser. When Steam redirects back to our
/// (never-served) return URL, we cancel that navigation, read the SteamID64 out of the query,
/// verify the assertion with Steam, and close with a result. The WebView2 keeps its profile in
/// %AppData% so a "remember me" session survives between launches.
/// </summary>
internal partial class SteamOpenIdLoginWindow : Window
{
    private static readonly HttpClient HttpClient = new();

    public SteamOpenIdLoginWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>The resolved SteamID64 once sign-in succeeds; null until then.</summary>
    public string? SteamId64 { get; private set; }

    /// <summary>The captured steamcommunity.com cookies (for authenticated calls), if any.</summary>
    public string? CookieHeader { get; private set; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CStoValuation", "WebView2");
            Directory.CreateDirectory(userDataFolder);

            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            await LoginBrowser.EnsureCoreWebView2Async(environment);

            LoginBrowser.NavigationStarting += OnNavigationStarting;
            LoginBrowser.Source = new Uri(SteamOpenId.BuildLoginUrl());
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or WebView2RuntimeNotFoundException)
        {
            // WebView2 runtime missing or failed to initialise — fall back to manual entry.
            MessageBox.Show(
                this,
                "Couldn't start the embedded browser (the WebView2 runtime may be missing). " +
                "You can still paste your SteamID64 or profile URL instead.",
                "Sign in unavailable",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            DialogResult = false;
        }
    }

    private async void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) || !SteamOpenId.IsReturnUrl(uri))
        {
            return;
        }

        // Stop the browser from actually navigating to our placeholder return URL.
        e.Cancel = true;

        var steamId = SteamOpenId.ExtractSteamId(uri);
        if (steamId is not null && await SteamOpenId.VerifyAsync(uri, HttpClient, CancellationToken.None))
        {
            SteamId64 = steamId;
            await CaptureSessionCookiesAsync();
            DialogResult = true;
        }
        else
        {
            DialogResult = false;
        }
    }

    /// <summary>
    /// Reads the steamcommunity.com cookies the browser holds after a successful login so the app
    /// can make authenticated calls (the Market price-history endpoint needs a logged-in session).
    /// </summary>
    private async Task CaptureSessionCookiesAsync()
    {
        try
        {
            var cookies = await LoginBrowser.CoreWebView2.CookieManager
                .GetCookiesAsync("https://steamcommunity.com");
            CookieHeader = string.Join(
                "; ",
                cookies.Where(c => !string.IsNullOrEmpty(c.Value)).Select(c => $"{c.Name}={c.Value}"));
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            CookieHeader = null;
        }
    }
}
