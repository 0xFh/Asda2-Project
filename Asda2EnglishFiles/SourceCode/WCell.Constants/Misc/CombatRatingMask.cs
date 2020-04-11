using System;

namespace WCell.Constants.Misc
{
    /// <summary>Used for AuraType: ModRating</summary>
    [Flags]
    public enum CombatRatingMask : uint
    {
        Weapon = 1,
        Defence = 2,
        Dodge = 4,
        Parry = 8,
        Block = 16, // 0x00000010
        MeleeHitChance = 32, // 0x00000020
        RangedHitChance = 64, // 0x00000040
        SpellHitChance = 128, // 0x00000080
        MeleeCritical = 256, // 0x00000100
        RangedCritical = 512, // 0x00000200
        SpellCritical = 1024, // 0x00000400
        MeleeResilience = 16384, // 0x00004000
        RangedResilience = 32768, // 0x00008000
        SpellResilience = 65536, // 0x00010000
        MeleeHaste = 131072, // 0x00020000
        RangedHaste = 262144, // 0x00040000
        SpellHaste = 524288, // 0x00080000
        WeaponSkillMainhand = 1048576, // 0x00100000
        WeaponSkillOffhand = 2097152, // 0x00200000
        WeaponSkillRanged = 4194304, // 0x00400000
        Expertise = 8388608, // 0x00800000
        ArmorPenetration = 16777216, // 0x01000000
    }
}