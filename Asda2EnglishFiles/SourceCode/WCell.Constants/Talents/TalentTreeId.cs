using System;

namespace WCell.Constants.Talents
{
    [Serializable]
    public enum TalentTreeId : uint
    {
        None = 0,
        MageFire = 41, // 0x00000029
        MageFrost = 61, // 0x0000003D
        MageArcane = 81, // 0x00000051
        WarriorArms = 161, // 0x000000A1
        WarriorProtection = 163, // 0x000000A3
        WarriorFury = 164, // 0x000000A4
        RogueCombat = 181, // 0x000000B5
        RogueAssassination = 182, // 0x000000B6
        RogueSubtlety = 183, // 0x000000B7
        PriestDiscipline = 201, // 0x000000C9
        PriestHoly = 202, // 0x000000CA
        PriestShadow = 203, // 0x000000CB
        ShamanElemental = 261, // 0x00000105
        ShamanRestoration = 262, // 0x00000106
        ShamanEnhancement = 263, // 0x00000107
        DruidFeralCombat = 281, // 0x00000119
        DruidRestoration = 282, // 0x0000011A
        DruidBalance = 283, // 0x0000011B
        WarlockDestruction = 301, // 0x0000012D
        WarlockAffliction = 302, // 0x0000012E
        WarlockDemonology = 303, // 0x0000012F
        HunterBeastMastery = 361, // 0x00000169
        HunterSurvival = 362, // 0x0000016A
        HunterMarksmanship = 363, // 0x0000016B
        PaladinRetribution = 381, // 0x0000017D
        PaladinHoly = 382, // 0x0000017E
        PaladinProtection = 383, // 0x0000017F
        DeathKnightBlood = 398, // 0x0000018E
        DeathKnightFrost = 399, // 0x0000018F
        DeathKnightUnholy = 400, // 0x00000190
        PetTalentsTenacity = 409, // 0x00000199
        PetTalentsFerocity = 410, // 0x0000019A
        PetTalentsCunning = 411, // 0x0000019B
        End = 412, // 0x0000019C
    }
}