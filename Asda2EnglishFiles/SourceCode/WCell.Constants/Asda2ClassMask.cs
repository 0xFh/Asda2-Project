using System;

namespace WCell.Constants
{
    [Flags]
    public enum Asda2ClassMask
    {
        All = 0,
        OHS = 2,
        Spear = 4,
        THS = 8,
        Mage = 32, // 0x00000020
        Crossbow = 512, // 0x00000200
        Bow = 1024, // 0x00000400
        Balista = 2048, // 0x00000800
    }
}