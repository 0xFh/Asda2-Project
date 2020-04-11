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
        Mage = 32,
        Crossbow = 512,
        Bow = 1024,
        Balista = 2048
    }
}