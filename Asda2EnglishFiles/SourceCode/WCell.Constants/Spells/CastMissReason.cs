namespace WCell.Constants.Spells
{
    /// <summary>
    /// In-game displayed above target head when spell fails to affect target
    /// </summary>
    public enum CastMissReason : byte
    {
        None,
        Miss,
        Resist,
        Dodge,
        Parry,
        Block,
        Evade,
        Immune,
        Immune_2,
        Deflect,
        Absorb,
        Reflect,
    }
}