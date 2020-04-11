using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum SpellLogFlags
    {
        None = 0,
        SpellLogFlag_0x1 = 1,
        Critical = 2,
        SpellLogFlag_0x4 = 4,
        SpellLogFlag_0x8 = 8,
        SpellLogFlag_0x10 = 16, // 0x00000010
        SpellLogFlag_0x20 = 32, // 0x00000020
    }
}