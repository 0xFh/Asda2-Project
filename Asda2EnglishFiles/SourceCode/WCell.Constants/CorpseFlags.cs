using System;

namespace WCell.Constants
{
    [Flags]
    public enum CorpseFlags
    {
        None = 0,
        IsClaimed = 1,
        CorpseFlag_0x0002 = 2,
        Bones = 4,
        CorpseFlag_0x0008 = 8,
        CorpseFlag_0x0010 = 16, // 0x00000010
        CorpseFlag_0x0020 = 32, // 0x00000020
        CorpseFlag_0x0040 = 64, // 0x00000040
    }
}