using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum ChannelInterruptFlags
    {
        None = 0,
        ChannelInterruptOn1 = 1,
        ChannelInterruptOn2 = 2,
        ChannelInterruptOn3 = 4,
        ChannelInterruptOn4 = 8,
        ChannelInterruptOn5 = 16, // 0x00000010
        ChannelInterruptOn6 = 32, // 0x00000020
        ChannelInterruptOn7 = 64, // 0x00000040
        ChannelInterruptOn8 = 128, // 0x00000080
        ChannelInterruptOn9 = 256, // 0x00000100
        ChannelInterruptOn10 = 512, // 0x00000200
        ChannelInterruptOn11 = 1024, // 0x00000400
        ChannelInterruptOn12 = 2048, // 0x00000800
        ChannelInterruptOn13 = 4096, // 0x00001000
        ChannelInterruptOn14 = 8192, // 0x00002000
        ChannelInterruptOn15 = 16384, // 0x00004000
        ChannelInterruptOn16 = 32768, // 0x00008000
        ChannelInterruptOn17 = 65536, // 0x00010000
        ChannelInterruptOn18 = 131072, // 0x00020000
    }
}