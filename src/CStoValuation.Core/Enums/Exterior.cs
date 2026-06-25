namespace CStoValuation.Core.Enums;

/// <summary>
/// The wear/condition of a CS2 skin, from best to worst. <see cref="None"/>
/// covers items that simply have no exterior (cases, stickers, agents).
/// Numeric ordering mirrors the in-game quality scale.
/// </summary>
public enum Exterior
{
    /// <summary>Item has no exterior (e.g. a case or sticker).</summary>
    None = 0,
    FactoryNew = 1,
    MinimalWear = 2,
    FieldTested = 3,
    WellWorn = 4,
    BattleScarred = 5,
}
