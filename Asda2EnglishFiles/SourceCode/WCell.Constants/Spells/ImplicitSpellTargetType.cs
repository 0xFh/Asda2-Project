using System;

namespace WCell.Constants.Spells
{
    [Serializable]
    public enum ImplicitSpellTargetType
    {
        None = 0,
        Self = 1,
        Type_2 = 2,
        InvisibleOrHiddenEnemiesAtLocationRadius = 3,
        SpreadableDesease = 4,
        Pet = 5,
        SingleEnemy = 6,
        ScriptedTarget = 7,
        AllAroundLocation = 8,
        HeartstoneLocation = 9,
        Type_11 = 11, // 0x0000000B
        AllEnemiesInArea = 15, // 0x0000000F
        AllEnemiesInAreaInstant = 16, // 0x00000010
        TeleportLocation = 17, // 0x00000011
        LocationToSummon = 18, // 0x00000012
        AllPartyAroundCaster = 20, // 0x00000014
        SingleFriend = 21, // 0x00000015
        AllEnemiesAroundCaster = 22, // 0x00000016
        GameObject = 23, // 0x00000017
        InFrontOfCaster = 24, // 0x00000018
        Duel = 25, // 0x00000019
        GameObjectOrItem = 26, // 0x0000001A
        PetMaster = 27, // 0x0000001B
        AllEnemiesInAreaChanneled = 28, // 0x0000001C
        AllPartyInAreaChanneled = 29, // 0x0000001D
        AllFriendlyInAura = 30, // 0x0000001E
        AllTargetableAroundLocationInRadiusOverTime = 31, // 0x0000001F
        Minion = 32, // 0x00000020
        AllPartyInArea = 33, // 0x00000021
        Tranquility = 34, // 0x00000022
        SingleParty = 35, // 0x00000023
        PetSummonLocation = 36, // 0x00000024
        AllParty = 37, // 0x00000025
        ScriptedOrSingleTarget = 38, // 0x00000026
        SelfFishing = 39, // 0x00000027
        ScriptedGameObject = 40, // 0x00000028
        TotemEarth = 41, // 0x00000029
        TotemWater = 42, // 0x0000002A
        TotemAir = 43, // 0x0000002B
        TotemFire = 44, // 0x0000002C
        Chain = 45, // 0x0000002D
        ScriptedObjectLocation = 46, // 0x0000002E
        DynamicObject = 47, // 0x0000002F
        MultipleSummonLocation = 48, // 0x00000030
        MultipleSummonPetLocation = 49, // 0x00000031
        SummonLocation = 50, // 0x00000032
        CaliriEggs = 51, // 0x00000033
        LocationNearCaster = 52, // 0x00000034
        CurrentSelection = 53, // 0x00000035
        TargetAtOrientationOfCaster = 54, // 0x00000036
        LocationInFrontCaster = 55, // 0x00000037
        PartyAroundCaster = 56, // 0x00000038
        PartyMember = 57, // 0x00000039
        Type_58 = 58, // 0x0000003A
        TargetForVisualEffect = 59, // 0x0000003B
        ScriptedTarget2 = 60, // 0x0000003C
        AreaEffectPartyAndClass = 61, // 0x0000003D
        NatureSummonLocation = 63, // 0x0000003F
        InFrontOfTargetLocation = 64, // 0x00000040
        BehindTargetLocation = 65, // 0x00000041
        RightFromTargetLocation = 66, // 0x00000042
        LeftFromTargetLocation = 67, // 0x00000043
        Type_68 = 68, // 0x00000044
        Type_69 = 69, // 0x00000045
        Type_70 = 70, // 0x00000046
        MultipleGuardianSummonLocation = 72, // 0x00000048
        NetherDrakeSummonLocation = 73, // 0x00000049
        ScriptedLocation = 74, // 0x0000004A
        LocationInFrontCasterAtRange = 75, // 0x0000004B
        SelectedEnemyChanneled = 77, // 0x0000004D
        Type_78 = 78, // 0x0000004E
        Type_79 = 79, // 0x0000004F
        Type_80 = 80, // 0x00000050
        Type_81 = 81, // 0x00000051
        Type_85 = 85, // 0x00000055
        SelectedEnemyDeadlyPoison = 86, // 0x00000056
        Type_87 = 87, // 0x00000057
        Type_88 = 88, // 0x00000058
        Type_89 = 89, // 0x00000059
        Type_90 = 90, // 0x0000005A
        Type_91 = 91, // 0x0000005B
        Type_93 = 93, // 0x0000005D
        ConeInFrontOfCaster = 104, // 0x00000068
        Target_105 = 105, // 0x00000069
    }
}