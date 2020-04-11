using System;

namespace WCell.Constants
{
    [Flags]
    public enum ZoneFlags : uint
    {
        None = 0,
        Unk_0x1 = 1,
        Unk_0x2 = 2,
        Unk_0x4 = 4,
        Capital = 8,
        Unk_0x10 = 16, // 0x00000010
        Capital2 = 32, // 0x00000020
        Duel = 64, // 0x00000040
        Arena = 128, // 0x00000080
        CapitalCity = 256, // 0x00000100
        Unk_0x200 = 512, // 0x00000200
        CanFly = 1024, // 0x00000400
        Sanctuary = 2048, // 0x00000800
        RespawnNoCorpse = 4096, // 0x00001000
        Unk_0x4000 = 16384, // 0x00004000
        Unused_0x8000 = 32768, // 0x00008000
        InstancedArena = 65536, // 0x00010000
        Unused_0x40000 = 262144, // 0x00040000
        PlayableFactionCapital = 2097152, // 0x00200000
        OutdoorPvP = 16777216, // 0x01000000
        Unk_0x2000000 = 33554432, // 0x02000000
        Unk_0x4000000 = 67108864, // 0x04000000
        CanHearthAndResurrectFromArea = 134217728, // 0x08000000
        CannotFly = 536870912, // 0x20000000
    }
}