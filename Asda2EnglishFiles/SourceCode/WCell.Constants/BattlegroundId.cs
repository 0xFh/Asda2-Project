using System;

namespace WCell.Constants
{
    [Serializable]
    public enum BattlegroundId : uint
    {
        None = 0,
        AlteracValley = 1,
        WarsongGulch = 2,
        ArathiBasin = 3,
        NagrandArena = 4,
        BladesEdgeArena = 5,
        AllArenas = 6,
        EyeOfTheStorm = 7,
        RuinsOfLordaeron = 8,
        StrandOfTheAncients = 9,
        DalaranSewers = 10, // 0x0000000A
        TheRingOfValor = 11, // 0x0000000B
        IsleOfConquest = 30, // 0x0000001E
        ABGUnknown = 31, // 0x0000001F
        End = 32, // 0x00000020
    }
}