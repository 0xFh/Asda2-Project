using System;

namespace WCell.Constants
{
    /// <summary>
    /// The mask is used when translating PacketIn RaceMask values.
    ///  Similar to RaceMask but its values have twice the original value.
    /// </summary>
    [Flags]
    public enum RaceMask2 : uint
    {
        None = 0,
        Human = 2,
        Orc = 4,
        Dwarf = 8,
        NightElf = 16, // 0x00000010
        Undead = 32, // 0x00000020
        Tauren = 64, // 0x00000040
        Gnome = 128, // 0x00000080
        Troll = 256, // 0x00000100
        Goblin = 512, // 0x00000200
        BloodElf = 1024, // 0x00000400
        Draenei = 2048, // 0x00000800
        FelOrc = 4096, // 0x00001000
        Naga = 8192, // 0x00002000
        Broken = 16384, // 0x00004000
        Skeleton = 32768, // 0x00008000
        All = 4294967295, // 0xFFFFFFFF
    }
}