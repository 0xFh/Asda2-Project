using System;

namespace WCell.Constants.Chat
{
    [Flags]
    public enum ChatChannelFlagsClient : uint
    {
        None = 0,
        Custom = 1,
        Trade = 4,
        FFA = 8,
        Predefined = 16, // 0x00000010
        CityOnly = 32, // 0x00000020
        LFG = 64, // 0x00000040
        Voice = 128, // 0x00000080
    }
}