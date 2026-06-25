using CStoValuation.Core.Models;

namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Fetches and maps a public CS2 inventory for a given account.
/// </summary>
public interface ISteamInventoryService
{
    /// <summary>
    /// Downloads the inventory, joins the raw assets to their descriptions, and maps
    /// the tags into a clean list of <see cref="InventoryItem"/>s.
    /// </summary>
    /// <param name="steamId64">The account to read.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <exception cref="Exceptions.PrivateInventoryException">If the inventory is private.</exception>
    Task<IReadOnlyList<InventoryItem>> GetInventoryAsync(
        string steamId64, CancellationToken cancellationToken = default);
}
