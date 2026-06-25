namespace CStoValuation.Core.Enums;

/// <summary>
/// CS2 item rarity tiers, ordered from most common to most rare. The explicit
/// numeric values make the ordering meaningful, so the UI can sort or colour by
/// rarity without a separate lookup table.
/// </summary>
public enum Rarity
{
    /// <summary>Rarity tag was missing or unrecognised.</summary>
    Unknown = 0,
    Consumer = 1,
    Industrial = 2,
    MilSpec = 3,
    Restricted = 4,
    Classified = 5,
    Covert = 6,
    Contraband = 7,

    /// <summary>Knives and gloves ("★" items) — Steam's "Extraordinary" tier.</summary>
    Extraordinary = 8,
}
