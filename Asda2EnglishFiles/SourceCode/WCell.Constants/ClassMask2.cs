using System;

namespace WCell.Constants
{
    /// <summary>
    /// The mask is used when translating PacketIn ClassMask values.
    /// Similar to ClassMask but its values have twice the original value.
    /// </summary>
    [Flags]
    public enum ClassMask2 : uint
    {
        None = 0,
        Warrior = 2,
        Paladin = 4,
        Hunter = 8,
        Rogue = 16, // 0x00000010
        Priest = 32, // 0x00000020
        Shaman = 128, // 0x00000080
        Mage = 256, // 0x00000100
        Warlock = 512, // 0x00000200
        Druid = 2048, // 0x00000800
        All = 4294967295, // 0xFFFFFFFF
    }
}