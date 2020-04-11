using System;

namespace WCell.Constants
{
    /// <summary>The mask is the corrosponding ClassId ^2 - 1</summary>
    [Flags]
    [Serializable]
    public enum ClassMask : uint
    {
        None = 0,
        Warrior = 1,
        Paladin = 2,
        Hunter = 4,
        Rogue = 8,
        Priest = 16, // 0x00000010
        DeathKnight = 32, // 0x00000020
        Shaman = 64, // 0x00000040
        Mage = 128, // 0x00000080
        Warlock = 256, // 0x00000100
        Druid = 1024, // 0x00000400
        AllClasses1 = 32767, // 0x00007FFF
        AllClasses2 = 4294967295, // 0xFFFFFFFF
    }
}