using System;

namespace WCell.RealmServer.Spells
{
    [Flags]
    public enum ShapeshiftInfoFlags : uint
    {
        NotActualShapeshift = 1,
        AgilityBasedAttackPower = 32, // 0x00000020
    }
}