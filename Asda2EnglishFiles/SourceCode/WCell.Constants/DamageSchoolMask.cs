using System;

namespace WCell.Constants
{
    [Flags]
    public enum DamageSchoolMask : uint
    {
        None = 0,
        Physical = 1,
        Holy = 2,
        Fire = 4,
        Nature = 8,
        Frost = 16, // 0x00000010
        Shadow = 32, // 0x00000020
        Arcane = 64, // 0x00000040
        Magical = 128, // 0x00000080
        MagicSchools = Arcane | Shadow | Frost | Nature | Fire | Holy, // 0x0000007E
        AllSchools = MagicSchools | Physical, // 0x0000007F
    }
}