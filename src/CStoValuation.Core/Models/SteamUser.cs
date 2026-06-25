namespace CStoValuation.Core.Models;

/// <summary>
/// The Steam account whose inventory we are valuing. A plain immutable value:
/// once resolved it never changes within a session.
/// </summary>
/// <param name="SteamId64">The canonical 17-digit SteamID64.</param>
/// <param name="PersonaName">The public display name, when known.</param>
/// <param name="AvatarUrl">URL of the profile avatar, when known.</param>
public sealed record SteamUser(string SteamId64, string? PersonaName, string? AvatarUrl);
