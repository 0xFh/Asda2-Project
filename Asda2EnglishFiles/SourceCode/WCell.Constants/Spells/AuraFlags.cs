using System;

namespace WCell.Constants.Spells
{
    /// <summary>8 Bit flags</summary>
    [Flags]
    public enum AuraFlags : byte
    {
        None = 0,
        Effect1AppliesAura = 1,
        Effect2AppliesAura = 2,
        Effect3AppliesAura = 4,
        TargetIsCaster = 8,
        Positive = 16, // 0x10
        HasDuration = 32, // 0x20
        Flag_0x40 = 64, // 0x40
        Negative = 128, // 0x80
    }
}