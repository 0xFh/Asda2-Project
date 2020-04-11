using System;

namespace WCell.Constants.Login
{
    [Flags]
    public enum RealmFlags : byte
    {
        None = 0,
        RedName = 1,
        Offline = 2,
        SpecifyBuild = 4,
        Unk1 = 8,
        Unk2 = 16, // 0x10
        NewPlayers = 32, // 0x20
        Recommended = 64, // 0x40
        Full = 128, // 0x80
    }
}