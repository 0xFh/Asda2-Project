using System;

namespace WCell.Constants.Items
{
    [Flags]
    public enum ItemSubClassMask : uint
    {
        None = 0,
        WeaponAxe = 1,
        WeaponTwoHandAxe = 2,
        WeaponBow = 4,
        WeaponGun = 8,
        WeaponPolearm = 16, // 0x00000010
        WeaponTwoHandMace = 32, // 0x00000020
        Shield = 64, // 0x00000040
        WeaponOneHandSword = 128, // 0x00000080
        WeaponTwoHandSword = 256, // 0x00000100
        UnknownSubClass1 = 512, // 0x00000200
        WeaponStaff = 1024, // 0x00000400
        WeaponFist = 8192, // 0x00002000
        WeaponDagger = 32768, // 0x00008000
        WeaponThrown = 65536, // 0x00010000
        UnknownSubClass2 = 131072, // 0x00020000
        WeaponCrossbow = 262144, // 0x00040000
        WeaponWand = 524288, // 0x00080000
        WeaponFishingPole = 1048576, // 0x00100000
        ArmorMisc = WeaponAxe, // 0x00000001
        ArmorCloth = WeaponTwoHandAxe, // 0x00000002
        ArmorLeather = WeaponBow, // 0x00000004
        ArmorMail = WeaponGun, // 0x00000008
        ArmorPlate = WeaponPolearm, // 0x00000010
        ArmorShield = Shield, // 0x00000040

        AnyMeleeWeapon =
            ArmorShield | ArmorPlate | ArmorCloth | ArmorMisc | UnknownSubClass2 | WeaponDagger | WeaponFist |
            WeaponStaff | WeaponTwoHandSword | WeaponOneHandSword | WeaponTwoHandMace, // 0x0002A5F3
        AnyRangedWeapon = ArmorMail | ArmorLeather | WeaponCrossbow, // 0x0004000C
        AnyRangedAndThrownWeapon = AnyRangedWeapon | WeaponThrown, // 0x0005000C
    }
}