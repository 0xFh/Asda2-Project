using System;

namespace WCell.Constants
{
    [Flags]
    public enum UnitExtraFlags
    {
        None = 0,
        InstanceBind = 1,
        Civilian = 2,
        NoParry = 4,
        NoParryHaste = 8,
        NoBlock = 16, // 0x00000010
        NoCrush = 32, // 0x00000020
        NoXP = 64, // 0x00000040
        Invisible = 128, // 0x00000080
        NoTaunt = 256, // 0x00000100
        Ghost = 512, // 0x00000200
    }
}