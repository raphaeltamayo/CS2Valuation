using CStoValuation.Core.Models;

namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Caches the most recently imported inventory per account, so the app can show the last
/// known holdings instantly on launch and when the network is unavailable.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>Replaces the cached inventory for an account with a fresh import.</summary>
    Task SaveInventoryAsync(
        string steamId64, IReadOnlyCollection<InventoryItem> items,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the cached inventory for an account, or an empty list if none.</summary>
    Task<IReadOnlyList<InventoryItem>> GetCachedInventoryAsync(
        string steamId64, CancellationToken cancellationToken = default);
}
