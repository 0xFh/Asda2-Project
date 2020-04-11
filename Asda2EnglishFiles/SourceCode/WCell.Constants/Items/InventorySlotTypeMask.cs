using System;

namespace WCell.Constants.Items
{
    [Flags]
    public enum InventorySlotTypeMask
    {
        None = 0,
        Head = 2,
        Neck = 4,
        Shoulder = 8,
        Body = 16, // 0x00000010
        Chest = 32, // 0x00000020
        Waist = 64, // 0x00000040
        Legs = 128, // 0x00000080
        Feet = 256, // 0x00000100
        Wrist = 512, // 0x00000200
        Hand = 1024, // 0x00000400
        Finger = 2048, // 0x00000800
        Trinket = 4096, // 0x00001000
        Weapon = 8192, // 0x00002000
        Shield = 16384, // 0x00004000
        WeaponRanged = 32768, // 0x00008000
        Cloak = 65536, // 0x00010000
        TwoHandWeapon = 131072, // 0x00020000
        Bag = 262144, // 0x00040000
        Tabard = 524288, // 0x00080000
        Robe = 1048576, // 0x00100000
        WeaponMainHand = 2097152, // 0x00200000
        WeaponOffHand = 4194304, // 0x00400000
        Holdable = 8388608, // 0x00800000
        Ammo = 16777216, // 0x01000000
        Thrown = 33554432, // 0x02000000
        RangedRight = 67108864, // 0x04000000
        Quiver = 134217728, // 0x08000000
        Relic = 268435456, // 0x10000000
    }
}