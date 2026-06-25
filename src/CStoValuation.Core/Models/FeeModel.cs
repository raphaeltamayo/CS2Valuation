namespace CStoValuation.Core.Models;

/// <summary>
/// Converts a gross market price into the seller's realizable proceeds by applying
/// a transaction fee. This is the single place the gross→net relationship lives, so
/// the whole app shares one definition of "what would I actually pocket if I sold".
/// </summary>
/// <remarks>
/// Skinport's standard seller fee is 8% (lower for high-value items). The rate is a
/// constructor parameter so the model is configurable and trivially unit-testable.
/// </remarks>
public sealed record FeeModel
{
    /// <summary>The default Skinport seller fee (8%).</summary>
    public static FeeModel Default { get; } = new(0.08m);

    /// <summary>The fractional seller fee, e.g. 0.08 for 8%. In [0, 1).</summary>
    public decimal SellerFeeRate { get; }

    /// <summary>Creates a fee model, rejecting nonsensical rates up front.</summary>
    /// <param name="sellerFeeRate">Fractional fee in the half-open range [0, 1).</param>
    /// <exception cref="ArgumentOutOfRangeException">If the rate is negative or ≥ 1.</exception>
    public FeeModel(decimal sellerFeeRate)
    {
        // Guard clause: validate at construction so an invalid fee can never
        // propagate silently into a valuation total.
        if (sellerFeeRate is < 0m or >= 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sellerFeeRate), sellerFeeRate,
                "Seller fee rate must be in the range [0, 1).");
        }

        SellerFeeRate = sellerFeeRate;
    }

    /// <summary>
    /// Applies the fee to a gross amount and returns the net proceeds, rounded to
    /// two decimal places (banker's-safe, away-from-zero, like a real settlement).
    /// </summary>
    public decimal NetFromGross(decimal gross) =>
        decimal.Round(gross * (1m - SellerFeeRate), 2, MidpointRounding.AwayFromZero);
}
