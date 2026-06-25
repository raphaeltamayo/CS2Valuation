namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Turns whatever a user pastes — a raw SteamID64, a <c>/profiles/{id}/</c> URL, or a
/// <c>/id/{vanity}/</c> custom URL — into a canonical SteamID64. Vanity resolution may
/// hit the network (Steam's <c>?xml=1</c> endpoint), hence the asynchronous signature.
/// </summary>
public interface ISteamIdResolver
{
    /// <summary>Resolves user input to a 17-digit SteamID64.</summary>
    /// <param name="input">A SteamID64, profile URL, or vanity URL.</param>
    /// <param name="cancellationToken">Cancels an in-flight vanity lookup.</param>
    /// <returns>The canonical SteamID64.</returns>
    /// <exception cref="System.FormatException">If the input can't be resolved to an id.</exception>
    Task<string> ResolveAsync(string input, CancellationToken cancellationToken = default);
}
