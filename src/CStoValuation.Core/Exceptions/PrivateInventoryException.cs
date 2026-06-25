namespace CStoValuation.Core.Exceptions;

/// <summary>
/// Thrown when an inventory cannot be read because the Steam profile keeps it private
/// (HTTP 403 or a <c>success: 0</c> body). This is an expected, user-actionable
/// condition — not a bug — so it gets its own type the UI can catch and turn into a
/// friendly "make your inventory public" message rather than a generic error.
/// </summary>
public sealed class PrivateInventoryException : Exception
{
    /// <summary>The SteamID64 whose inventory was private.</summary>
    public string SteamId64 { get; }

    public PrivateInventoryException(string steamId64, string? message = null, Exception? innerException = null)
        : base(message ?? $"The inventory for Steam account {steamId64} is private.", innerException)
    {
        SteamId64 = steamId64;
    }
}
