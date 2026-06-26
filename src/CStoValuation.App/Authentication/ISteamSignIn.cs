namespace CStoValuation.App.Authentication;

/// <summary>
/// Abstracts the interactive "Sign in through Steam" gesture so the view-model can trigger it
/// without knowing about windows or WebView2 — keeping the view-model free of UI types and
/// independently testable.
/// </summary>
internal interface ISteamSignIn
{
    /// <summary>Runs the sign-in flow and returns the SteamID64, or null if the user cancelled.</summary>
    Task<string?> SignInAsync();
}
