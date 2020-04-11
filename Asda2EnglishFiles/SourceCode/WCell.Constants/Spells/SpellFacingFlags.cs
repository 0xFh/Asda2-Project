using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum SpellFacingFlags
    {
        RequiresInFront = 1,
        Flag_1_0x2 = 2,
        Flag_2_0x4 = 4,
        Flag_3_0x8 = 8,
    }
}