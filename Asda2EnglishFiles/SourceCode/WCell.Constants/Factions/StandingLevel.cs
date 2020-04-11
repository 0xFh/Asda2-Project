namespace WCell.Constants.Factions
{
    /// <summary>
    /// The level of each standing (0 - hated to 7 - exhalted)
    /// </summary>
    public enum StandingLevel : uint
    {
        Hated = 0,
        Unknown = 0,
        Hostile = 1,
        Unfriendly = 2,
        Neutral = 3,
        Friendly = 4,
        Honored = 5,
        Revered = 6,
        Exalted = 7,
    }
}