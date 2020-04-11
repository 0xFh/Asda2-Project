namespace WCell.Constants.Misc
{
    public enum CombatRating : uint
    {
        WeaponSkill = 1,
        DefenseSkill = 2,
        Dodge = 3,
        Parry = 4,
        Block = 5,
        MeleeHitChance = 6,
        RangedHitChance = 7,
        SpellHitChance = 8,
        MeleeCritChance = 9,
        RangedCritChance = 10, // 0x0000000A
        SpellCritChance = 11, // 0x0000000B
        MeleeAttackerHit = 12, // 0x0000000C
        RangedAttackerHit = 13, // 0x0000000D
        SpellAttackerHit = 14, // 0x0000000E
        MeleeResilience = 15, // 0x0000000F
        RangedResilience = 16, // 0x00000010
        SpellResilience = 17, // 0x00000011
        MeleeHaste = 18, // 0x00000012
        RangedHaste = 19, // 0x00000013
        SpellHaste = 20, // 0x00000014
        WeaponSkillMainhand = 21, // 0x00000015
        WeaponSkillOffhand = 22, // 0x00000016
        WeaponSkillRanged = 23, // 0x00000017
        Expertise = 24, // 0x00000018
        ArmorPenetration = 25, // 0x00000019
    }
}