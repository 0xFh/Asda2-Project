using System;

namespace WCell.Constants.Misc
{
    [Serializable]
    public enum ChatLanguage : uint
    {
        Universal = 0,
        Orcish = 1,
        Darnassian = 2,
        Taurahe = 3,
        Dwarven = 6,
        Common = 7,
        DemonTongue = 8,
        Titan = 9,
        Thalassian = 10, // 0x0000000A
        Draconic = 11, // 0x0000000B
        OldTongue = 12, // 0x0000000C
        Gnomish = 13, // 0x0000000D
        Troll = 14, // 0x0000000E
        Gutterspeak = 33, // 0x00000021
        Draenei = 35, // 0x00000023
        End = 36, // 0x00000024
        Universal2 = 4294967295, // 0xFFFFFFFF
    }
}