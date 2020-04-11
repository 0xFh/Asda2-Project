using System;

namespace WCell.Constants
{
    /// <summary>
    /// Mask for
    /// <remarks>Values are from column 36 of CharTitles.dbc. (1 lsh value) </remarks>
    /// </summary>
    [Flags]
    public enum CharTitlesMask : ulong
    {
        Disabled = 0,
        None = 1,
        Private = 2,
        Corporal = 4,
        SergeantA = 8,
        MasterSergeant = 16, // 0x0000000000000010
        SergeantMajor = 32, // 0x0000000000000020
        Knight = 64, // 0x0000000000000040
        KnightLieutenant = 128, // 0x0000000000000080
        KnightCaptain = 256, // 0x0000000000000100
        KnightChampion = 512, // 0x0000000000000200
        LieutenantCommander = 1024, // 0x0000000000000400
        Commander = 2048, // 0x0000000000000800
        Marshal = 4096, // 0x0000000000001000
        FieldMarshal = 8192, // 0x0000000000002000
        GrandMarshal = 16384, // 0x0000000000004000
        Scout = 32768, // 0x0000000000008000
        Grunt = 65536, // 0x0000000000010000
        SergeantH = 131072, // 0x0000000000020000
        SeniorSergeant = 262144, // 0x0000000000040000
        FirstSergeant = 524288, // 0x0000000000080000
        StoneGuard = 1048576, // 0x0000000000100000
        BloodGuard = 2097152, // 0x0000000000200000
        Legionnaire = 4194304, // 0x0000000000400000
        Centurion = 8388608, // 0x0000000000800000
        Champion = 16777216, // 0x0000000001000000
        LieutenantGeneral = 33554432, // 0x0000000002000000
        General = 67108864, // 0x0000000004000000
        Warlord = 134217728, // 0x0000000008000000
        HighWarlord = 268435456, // 0x0000000010000000
        Gladiator = 536870912, // 0x0000000020000000
        Duelist = 1073741824, // 0x0000000040000000
        Rival = 2147483648, // 0x0000000080000000
        Challenger = 4294967296, // 0x0000000100000000
        ScarabLord = 8589934592, // 0x0000000200000000
        Conqueror = 17179869184, // 0x0000000400000000
        Justicar = 34359738368, // 0x0000000800000000
        ChampionOfTheNaaru = 68719476736, // 0x0000001000000000
        MercilessGladiator = 137438953472, // 0x0000002000000000
        OfTheShatteredSun = 274877906944, // 0x0000004000000000
        HandOfAdal = 549755813888, // 0x0000008000000000
        VengefulGladiator = 1099511627776, // 0x0000010000000000
    }
}