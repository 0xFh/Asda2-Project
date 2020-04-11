using System;

namespace WCell.Constants
{
    /// <summary>The mask is the corrosponding RaceTypes-value ^2 - 1</summary>
    [Flags]
    [Serializable]
    public enum RaceMask : uint
    {
        Human = 1,
        Orc = 2,
        Dwarf = 4,
        NightElf = 8,
        Undead = 16, // 0x00000010
        Tauren = 32, // 0x00000020
        Gnome = 64, // 0x00000040
        Troll = 128, // 0x00000080
        Goblin = 256, // 0x00000100
        BloodElf = 512, // 0x00000200
        Draenei = 1024, // 0x00000400
        FelOrc = 2048, // 0x00000800
        Naga = 4096, // 0x00001000
        Broken = 8192, // 0x00002000
        Skeleton = 16384, // 0x00004000
        AllRaces1 = 4294967295, // 0xFFFFFFFF

        AllRaces2 = Skeleton | Broken | Naga | FelOrc | Draenei | BloodElf | Goblin | Troll | Gnome | Tauren | Undead |
                    NightElf | Dwarf | Orc | Human, // 0x00007FFF
    }
}