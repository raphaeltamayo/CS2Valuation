using CStoValuation.Core.Models;

namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Persists price snapshots and the charting time-series, and reads them back for the
/// price chart and the "movers" calculation.
/// </summary>
public interface IPriceSnapshotRepository
{
    /// <summary>Appends a batch of full price snapshots.</summary>
    Task AddSnapshotsAsync(
        IEnumerable<PriceSnapshot> snapshots, CancellationToken cancellationToken = default);

    /// <summary>Appends a batch of charting history points.</summary>
    Task AddHistoryPointsAsync(
        IEnumerable<PriceHistoryPoint> points, CancellationToken cancellationToken = default);

    /// <summary>Returns an item's history points recorded at or after <paramref name="since"/>, oldest first.</summary>
    Task<IReadOnlyList<PriceHistoryPoint>> GetHistoryAsync(
        string marketHashName, DateTimeOffset since, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent snapshot for an item, or <c>null</c> if none exists.</summary>
    Task<PriceSnapshot?> GetLatestSnapshotAsync(
        string marketHashName, CancellationToken cancellationToken = default);
}
