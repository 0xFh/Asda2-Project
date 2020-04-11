using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum DisarmMask : uint
    {
        MainHand = 1,
        Ranged = 2,
        Offhand = Ranged | MainHand, // 0x00000003
    }
}