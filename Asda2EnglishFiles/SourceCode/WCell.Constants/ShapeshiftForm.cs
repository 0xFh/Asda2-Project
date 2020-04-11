namespace WCell.Constants
{
    /// <summary>
    /// Used in UNIT_FIELD_BYTES_2, 4th byte
    /// <remarks>Values from the first column of SpellShapeshiftForm.dbc</remarks>
    /// </summary>
    public enum ShapeshiftForm
    {
        Normal = 0,
        Cat = 1,
        TreeOfLife = 2,
        Travel = 3,
        Aqua = 4,
        Bear = 5,
        Ambient = 6,
        Ghoul = 7,
        DireBear = 8,
        CreatureBear = 14, // 0x0000000E
        CreatureCat = 15, // 0x0000000F
        GhostWolf = 16, // 0x00000010
        BattleStance = 17, // 0x00000011
        DefensiveStance = 18, // 0x00000012
        BerserkerStance = 19, // 0x00000013
        EpicFlightForm = 27, // 0x0000001B
        Shadow = 28, // 0x0000001C
        FlightForm = 29, // 0x0000001D
        Stealth = 30, // 0x0000001E
        Moonkin = 31, // 0x0000001F
        SpiritOfRedemption = 32, // 0x00000020
        End = 33, // 0x00000021
    }
}