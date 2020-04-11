using System;

namespace WCell.Constants.Spells
{
    [Serializable]
    public enum SummonFlags
    {
        None = 0,
        SummonFlags0x00000001 = 1,
        SummonFlags0x00000002 = 2,
        SummonFlags0x00000004 = 4,
        SummonFlags0x00000008 = 8,
        SummonFlags0x00000010 = 16, // 0x00000010
        SummonFlags0x00000020 = 32, // 0x00000020
        SummonFlags0x00000040 = 64, // 0x00000040
        SummonFlags0x00000080 = 128, // 0x00000080
        SummonFlags0x00000100 = 256, // 0x00000100
        SummonFlags0x00000200 = 512, // 0x00000200
        SummonFlags0x00000400 = 1024, // 0x00000400
        SummonFlags0x00000800 = 2048, // 0x00000800
        SummonFlags0x00001000 = 4096, // 0x00001000
        SummonFlags0x00002000 = 8192, // 0x00002000
        SummonFlags0x00004000 = 16384, // 0x00004000
        SummonFlags0x00008000 = 32768, // 0x00008000
        SummonFlags0x00010000 = 65536, // 0x00010000
        SummonFlags0x00020000 = 131072, // 0x00020000
        SummonFlags0x00040000 = 262144, // 0x00040000
        SummonFlags0x00080000 = 524288, // 0x00080000
        SummonFlags0x00100000 = 1048576, // 0x00100000
        SummonFlags0x00200000 = 2097152, // 0x00200000
        SummonFlags0x00400000 = 4194304, // 0x00400000
        SummonFlags0x00800000 = 8388608, // 0x00800000
    }
}