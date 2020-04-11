using System;

namespace WCell.Constants.Spells
{
    [Serializable]
    public enum SummonType
    {
        None = 0,
        Critter = 41, // 0x00000029
        Guardian = 61, // 0x0000003D
        TotemSlot1 = 63, // 0x0000003F
        Wild = 64, // 0x00000040
        Possessed = 65, // 0x00000041
        Demon = 66, // 0x00000042
        SummonPet = 67, // 0x00000043
        TotemSlot2 = 81, // 0x00000051
        TotemSlot3 = 82, // 0x00000052
        TotemSlot4 = 83, // 0x00000053
        Totem = 121, // 0x00000079
        Type_181 = 181, // 0x000000B5
        Type_187 = 187, // 0x000000BB
        Type_247 = 247, // 0x000000F7
        Critter2 = 307, // 0x00000133
        Critter3 = 407, // 0x00000197
        Type_409 = 409, // 0x00000199
        Type_427 = 427, // 0x000001AB
        SummonAndPossess = 428, // 0x000001AC
        Guardian2 = 713, // 0x000002C9
        Lightwell = 1141, // 0x00000475
        Guardian3 = 1161, // 0x00000489
        DoomGuard = 1221, // 0x000004C5
        Elemental = 1561, // 0x00000619
        ForceOfNature = 1562, // 0x0000061A
        End = 1563, // 0x0000061B
    }
}