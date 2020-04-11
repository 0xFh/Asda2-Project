using System;

namespace WCell.Constants.Login
{
    [Flags]
    public enum RealmServerType : byte
    {
        Normal = 0,
        PVP = 1,
        ServerType3 = 3,
        ServerType4 = 4,
        ServerType5 = ServerType4 | PVP, // 0x05
        RP = 6,
        RPPVP = RP | PVP, // 0x07
    }
}