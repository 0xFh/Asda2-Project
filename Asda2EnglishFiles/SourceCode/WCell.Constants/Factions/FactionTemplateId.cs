﻿using System;

namespace WCell.Constants.Factions
{
    [Serializable]
    public enum FactionTemplateId : uint
    {
        None = 0,
        PLAYERHuman = 1,
        PLAYEROrc = 2,
        PLAYERDwarf = 3,
        PLAYERNightElf = 4,
        PLAYERUndead = 5,
        PLAYERTauren = 6,
        Creature = 7,
        Escortee = 10, // 0x0000000A
        Stormwind = 11, // 0x0000000B
        Stormwind_2 = 12, // 0x0000000C
        Monster = 14, // 0x0000000E
        Creature_2 = 15, // 0x0000000F
        Monster_2 = 16, // 0x00000010
        DefiasBrotherhood = 17, // 0x00000011
        Murloc = 18, // 0x00000012
        GnollRedridge = 19, // 0x00000013
        GnollRiverpaw = 20, // 0x00000014
        UndeadScourge = 21, // 0x00000015
        BeastSpider = 22, // 0x00000016
        GnomereganExiles = 23, // 0x00000017
        Worgen = 24, // 0x00000018
        Kobold = 25, // 0x00000019
        Kobold_2 = 26, // 0x0000001A
        DefiasBrotherhood_2 = 27, // 0x0000001B
        TrollBloodscalp = 28, // 0x0000001C
        Orgrimmar = 29, // 0x0000001D
        TrollSkullsplitter = 30, // 0x0000001E
        Prey = 31, // 0x0000001F
        BeastWolf = 32, // 0x00000020
        Escortee_2 = 33, // 0x00000021
        DefiasBrotherhood_3 = 34, // 0x00000022
        Friendly = 35, // 0x00000023
        Trogg = 36, // 0x00000024
        TrollFrostmane = 37, // 0x00000025
        BeastWolf_2 = 38, // 0x00000026
        GnollShadowhide = 39, // 0x00000027
        OrcBlackrock = 40, // 0x00000028
        Villian = 41, // 0x00000029
        Victim = 42, // 0x0000002A
        Villian_2 = 43, // 0x0000002B
        BeastBear = 44, // 0x0000002C
        Ogre = 45, // 0x0000002D
        KurzensMercenaries = 46, // 0x0000002E
        VentureCompany = 47, // 0x0000002F
        BeastRaptor = 48, // 0x00000030
        Basilisk = 49, // 0x00000031
        DragonflightGreen = 50, // 0x00000032
        LostOnes = 51, // 0x00000033
        GizlocksDummy = 52, // 0x00000034
        HumanNightWatch = 53, // 0x00000035
        DarkIronDwarves = 54, // 0x00000036
        Ironforge = 55, // 0x00000037
        HumanNightWatch_2 = 56, // 0x00000038
        Ironforge_2 = 57, // 0x00000039
        Creature_3 = 58, // 0x0000003A
        Trogg_2 = 59, // 0x0000003B
        DragonflightRed = 60, // 0x0000003C
        GnollMosshide = 61, // 0x0000003D
        OrcDragonmaw = 62, // 0x0000003E
        GnomeLeper = 63, // 0x0000003F
        GnomereganExiles_2 = 64, // 0x00000040
        Orgrimmar_2 = 65, // 0x00000041
        Leopard = 66, // 0x00000042
        ScarletCrusade = 67, // 0x00000043
        Undercity = 68, // 0x00000044
        Ratchet = 69, // 0x00000045
        GnollRothide = 70, // 0x00000046
        Undercity_2 = 71, // 0x00000047
        BeastGorilla = 72, // 0x00000048
        BeastCarrionBird = 73, // 0x00000049
        Naga = 74, // 0x0000004A
        Dalaran = 76, // 0x0000004C
        ForlornSpirit = 77, // 0x0000004D
        Darkhowl = 78, // 0x0000004E
        Darnassus = 79, // 0x0000004F
        Darnassus_2 = 80, // 0x00000050
        Grell = 81, // 0x00000051
        Furbolg = 82, // 0x00000052
        HordeGeneric = 83, // 0x00000053
        AllianceGeneric = 84, // 0x00000054
        Orgrimmar_3 = 85, // 0x00000055
        GizlocksCharm = 86, // 0x00000056
        Syndicate = 87, // 0x00000057
        HillsbradMilitia = 88, // 0x00000058
        ScarletCrusade_2 = 89, // 0x00000059
        Demon = 90, // 0x0000005A
        Elemental = 91, // 0x0000005B
        Spirit = 92, // 0x0000005C
        Monster_3 = 93, // 0x0000005D
        Treasure = 94, // 0x0000005E
        GnollMudsnout = 95, // 0x0000005F
        HIllsbradSouthshoreMayor = 96, // 0x00000060
        Syndicate_2 = 97, // 0x00000061
        Undercity_3 = 98, // 0x00000062
        Victim_2 = 99, // 0x00000063
        Treasure_2 = 100, // 0x00000064
        Treasure_3 = 101, // 0x00000065
        Treasure_4 = 102, // 0x00000066
        DragonflightBlack = 103, // 0x00000067
        ThunderBluff = 104, // 0x00000068
        ThunderBluff_2 = 105, // 0x00000069
        HordeGeneric_2 = 106, // 0x0000006A
        TrollFrostmane_2 = 107, // 0x0000006B
        Syndicate_3 = 108, // 0x0000006C
        QuilboarRazormane2 = 109, // 0x0000006D
        QuilboarRazormane2_2 = 110, // 0x0000006E
        QuilboarBristleback = 111, // 0x0000006F
        QuilboarBristleback_2 = 112, // 0x00000070
        Escortee_3 = 113, // 0x00000071
        Treasure_5 = 114, // 0x00000072
        PLAYERGnome = 115, // 0x00000073
        PLAYERTroll = 116, // 0x00000074
        Undercity_4 = 118, // 0x00000076
        BloodsailBuccaneers = 119, // 0x00000077
        BootyBay = 120, // 0x00000078
        BootyBay_2 = 121, // 0x00000079
        Ironforge_3 = 122, // 0x0000007A
        Stormwind_3 = 123, // 0x0000007B
        Darnassus_3 = 124, // 0x0000007C
        Orgrimmar_4 = 125, // 0x0000007D
        DarkspearTrolls = 126, // 0x0000007E
        Villian_3 = 127, // 0x0000007F
        Blackfathom = 128, // 0x00000080
        Makrura = 129, // 0x00000081
        CentaurKolkar = 130, // 0x00000082
        CentaurGalak = 131, // 0x00000083
        GelkisClanCentaur = 132, // 0x00000084
        MagramClanCentaur = 133, // 0x00000085
        Maraudine = 134, // 0x00000086
        Monster_4 = 148, // 0x00000094
        Theramore = 149, // 0x00000095
        Theramore_2 = 150, // 0x00000096
        Theramore_3 = 151, // 0x00000097
        QuilboarRazorfen = 152, // 0x00000098
        QuilboarRazorfen_2 = 153, // 0x00000099
        QuilboarDeathshead = 154, // 0x0000009A
        Enemy = 168, // 0x000000A8
        Ambient = 188, // 0x000000BC
        Creature_4 = 189, // 0x000000BD
        Ambient_2 = 190, // 0x000000BE
        NethergardeCaravan = 208, // 0x000000D0
        NethergardeCaravan_2 = 209, // 0x000000D1
        AllianceGeneric_2 = 210, // 0x000000D2
        SouthseaFreebooters = 230, // 0x000000E6
        Escortee_4 = 231, // 0x000000E7
        Escortee_5 = 232, // 0x000000E8
        UndeadScourge_2 = 233, // 0x000000E9
        Escortee_6 = 250, // 0x000000FA
        WailingCaverns = 270, // 0x0000010E
        Escortee_7 = 290, // 0x00000122
        Silithid = 310, // 0x00000136
        Silithid_2 = 311, // 0x00000137
        BeastSpider_2 = 312, // 0x00000138
        WailingCaverns_2 = 330, // 0x0000014A
        Blackfathom_2 = 350, // 0x0000015E
        ArmiesOfCThun = 370, // 0x00000172
        SilvermoonRemnant = 371, // 0x00000173
        BootyBay_3 = 390, // 0x00000186
        Basilisk_2 = 410, // 0x0000019A
        BeastBat = 411, // 0x0000019B
        TheDefilers = 412, // 0x0000019C
        Scorpid = 413, // 0x0000019D
        TimbermawHold = 414, // 0x0000019E
        Titan = 415, // 0x0000019F
        Titan_2 = 416, // 0x000001A0
        TaskmasterFizzule = 430, // 0x000001AE
        WailingCaverns_3 = 450, // 0x000001C2
        Titan_3 = 470, // 0x000001D6
        Ravenholdt = 471, // 0x000001D7
        Syndicate_4 = 472, // 0x000001D8
        Ravenholdt_2 = 473, // 0x000001D9
        Gadgetzan = 474, // 0x000001DA
        Gadgetzan_2 = 475, // 0x000001DB
        GnomereganBug = 494, // 0x000001EE
        Escortee_8 = 495, // 0x000001EF
        Harpy = 514, // 0x00000202
        AllianceGeneric_3 = 534, // 0x00000216
        BurningBlade = 554, // 0x0000022A
        ShadowsilkPoacher = 574, // 0x0000023E
        SearingSpider = 575, // 0x0000023F
        Trogg_3 = 594, // 0x00000252
        Victim_3 = 614, // 0x00000266
        Monster_5 = 634, // 0x0000027A
        CenarionCircle = 635, // 0x0000027B
        TimbermawHold_2 = 636, // 0x0000027C
        Ratchet_2 = 637, // 0x0000027D
        TrollWitherbark = 654, // 0x0000028E
        CentaurKolkar_2 = 655, // 0x0000028F
        DarkIronDwarves_2 = 674, // 0x000002A2
        AllianceGeneric_4 = 694, // 0x000002B6
        HydraxianWaterlords = 695, // 0x000002B7
        HordeGeneric_3 = 714, // 0x000002CA
        DarkIronDwarves_3 = 734, // 0x000002DE
        GoblinDarkIronBarPatron = 735, // 0x000002DF
        GoblinDarkIronBarPatron_2 = 736, // 0x000002E0
        DarkIronDwarves_4 = 754, // 0x000002F2
        Escortee_9 = 774, // 0x00000306
        Escortee_10 = 775, // 0x00000307
        BroodOfNozdormu = 776, // 0x00000308
        MightOfKalimdor = 777, // 0x00000309
        Giant = 778, // 0x0000030A
        ArgentDawn = 794, // 0x0000031A
        TrollVilebranch = 795, // 0x0000031B
        ArgentDawn_2 = 814, // 0x0000032E
        Elemental_2 = 834, // 0x00000342
        Everlook = 854, // 0x00000356
        Everlook_2 = 855, // 0x00000357
        WintersaberTrainers = 874, // 0x0000036A
        GnomereganExiles_3 = 875, // 0x0000036B
        DarkspearTrolls_2 = 876, // 0x0000036C
        DarkspearTrolls_3 = 877, // 0x0000036D
        Theramore_4 = 894, // 0x0000037E
        TrainingDummy = 914, // 0x00000392
        FurbolgUncorrupted = 934, // 0x000003A6
        Demon_2 = 954, // 0x000003BA
        UndeadScourge_3 = 974, // 0x000003CE
        CenarionCircle_2 = 994, // 0x000003E2
        ThunderBluff_3 = 995, // 0x000003E3
        CenarionCircle_3 = 996, // 0x000003E4
        ShatterspearTrolls = 1014, // 0x000003F6
        ShatterspearTrolls_2 = 1015, // 0x000003F7
        HordeGeneric_4 = 1034, // 0x0000040A
        AllianceGeneric_5 = 1054, // 0x0000041E
        AllianceGeneric_6 = 1055, // 0x0000041F
        Orgrimmar_5 = 1074, // 0x00000432
        Theramore_5 = 1075, // 0x00000433
        Darnassus_4 = 1076, // 0x00000434
        Theramore_6 = 1077, // 0x00000435
        Stormwind_4 = 1078, // 0x00000436
        Friendly_2 = 1080, // 0x00000438
        Elemental_3 = 1081, // 0x00000439
        BeastBoar = 1094, // 0x00000446
        TrainingDummy_2 = 1095, // 0x00000447
        Theramore_7 = 1096, // 0x00000448
        Darnassus_5 = 1097, // 0x00000449
        DragonflightBlackBait = 1114, // 0x0000045A
        Undercity_5 = 1134, // 0x0000046E
        Undercity_6 = 1154, // 0x00000482
        Orgrimmar_6 = 1174, // 0x00000496
        BattlegroundNeutral = 1194, // 0x000004AA
        FrostwolfClan = 1214, // 0x000004BE
        FrostwolfClan_2 = 1215, // 0x000004BF
        StormpikeGuard = 1216, // 0x000004C0
        StormpikeGuard_2 = 1217, // 0x000004C1
        SulfuronFirelords = 1234, // 0x000004D2
        SulfuronFirelords_2 = 1235, // 0x000004D3
        SulfuronFirelords_3 = 1236, // 0x000004D4
        CenarionCircle_4 = 1254, // 0x000004E6
        Creature_5 = 1274, // 0x000004FA
        Creature_6 = 1275, // 0x000004FB
        Gizlock = 1294, // 0x0000050E
        HordeGeneric_5 = 1314, // 0x00000522
        AllianceGeneric_7 = 1315, // 0x00000523
        StormpikeGuard_3 = 1334, // 0x00000536
        FrostwolfClan_3 = 1335, // 0x00000537
        ShenDralar = 1354, // 0x0000054A
        ShenDralar_2 = 1355, // 0x0000054B
        OgreCaptainKromcrush = 1374, // 0x0000055E
        Treasure_6 = 1375, // 0x0000055F
        DragonflightBlack_2 = 1394, // 0x00000572
        SilithidAttackers = 1395, // 0x00000573
        SpiritGuideAlliance = 1414, // 0x00000586
        SpiritGuideHorde = 1415, // 0x00000587
        Jaedenar = 1434, // 0x0000059A
        Victim_4 = 1454, // 0x000005AE
        ThoriumBrotherhood = 1474, // 0x000005C2
        ThoriumBrotherhood_2 = 1475, // 0x000005C3
        HordeGeneric_6 = 1494, // 0x000005D6
        HordeGeneric_7 = 1495, // 0x000005D7
        HordeGeneric_8 = 1496, // 0x000005D8
        SilverwingSentinels = 1514, // 0x000005EA
        WarsongOutriders = 1515, // 0x000005EB
        StormpikeGuard_4 = 1534, // 0x000005FE
        FrostwolfClan_4 = 1554, // 0x00000612
        DarkmoonFaire = 1555, // 0x00000613
        ZandalarTribe = 1574, // 0x00000626
        Stormwind_5 = 1575, // 0x00000627
        SilvermoonRemnant_2 = 1576, // 0x00000628
        TheLeagueOfArathor = 1577, // 0x00000629
        Darnassus_6 = 1594, // 0x0000063A
        Orgrimmar_7 = 1595, // 0x0000063B
        StormpikeGuard_5 = 1596, // 0x0000063C
        FrostwolfClan_5 = 1597, // 0x0000063D
        TheDefilers_2 = 1598, // 0x0000063E
        TheLeagueOfArathor_2 = 1599, // 0x0000063F
        Darnassus_7 = 1600, // 0x00000640
        BroodOfNozdormu_2 = 1601, // 0x00000641
        SilvermoonCity = 1602, // 0x00000642
        SilvermoonCity_2 = 1603, // 0x00000643
        SilvermoonCity_3 = 1604, // 0x00000644
        DragonflightBronze = 1605, // 0x00000645
        Creature_7 = 1606, // 0x00000646
        Creature_8 = 1607, // 0x00000647
        CenarionCircle_5 = 1608, // 0x00000648
        PLAYERBloodElf = 1610, // 0x0000064A
        Ironforge_4 = 1611, // 0x0000064B
        Orgrimmar_8 = 1612, // 0x0000064C
        MightOfKalimdor_2 = 1613, // 0x0000064D
        Monster_6 = 1614, // 0x0000064E
        SteamwheedleCartel = 1615, // 0x0000064F
        RCObjects = 1616, // 0x00000650
        RCEnemies = 1617, // 0x00000651
        Ironforge_5 = 1618, // 0x00000652
        Orgrimmar_9 = 1619, // 0x00000653
        Enemy_2 = 1620, // 0x00000654
        Blue = 1621, // 0x00000655
        Red = 1622, // 0x00000656
        Tranquillien = 1623, // 0x00000657
        ArgentDawn_3 = 1624, // 0x00000658
        ArgentDawn_4 = 1625, // 0x00000659
        UndeadScourge_4 = 1626, // 0x0000065A
        Farstriders = 1627, // 0x0000065B
        Tranquillien_2 = 1628, // 0x0000065C
        PLAYERDraenei = 1629, // 0x0000065D
        ScourgeInvaders = 1630, // 0x0000065E
        ScourgeInvaders_2 = 1634, // 0x00000662
        SteamwheedleCartel_2 = 1635, // 0x00000663
        Farstriders_2 = 1636, // 0x00000664
        Farstriders_3 = 1637, // 0x00000665
        Exodar = 1638, // 0x00000666
        Exodar_2 = 1639, // 0x00000667
        Exodar_3 = 1640, // 0x00000668
        WarsongOutriders_2 = 1641, // 0x00000669
        SilverwingSentinels_2 = 1642, // 0x0000066A
        TrollForest = 1643, // 0x0000066B
        TheSonsOfLothar = 1644, // 0x0000066C
        TheSonsOfLothar_2 = 1645, // 0x0000066D
        Exodar_4 = 1646, // 0x0000066E
        Exodar_5 = 1647, // 0x0000066F
        TheSonsOfLothar_3 = 1648, // 0x00000670
        TheSonsOfLothar_4 = 1649, // 0x00000671
        TheMagHar = 1650, // 0x00000672
        TheMagHar_2 = 1651, // 0x00000673
        TheMagHar_3 = 1652, // 0x00000674
        TheMagHar_4 = 1653, // 0x00000675
        Exodar_6 = 1654, // 0x00000676
        Exodar_7 = 1655, // 0x00000677
        SilvermoonCity_4 = 1656, // 0x00000678
        SilvermoonCity_5 = 1657, // 0x00000679
        SilvermoonCity_6 = 1658, // 0x0000067A
        CenarionExpedition = 1659, // 0x0000067B
        CenarionExpedition_2 = 1660, // 0x0000067C
        CenarionExpedition_3 = 1661, // 0x0000067D
        FelOrc = 1662, // 0x0000067E
        FelOrcGhost = 1663, // 0x0000067F
        SonsOfLotharGhosts = 1664, // 0x00000680
        HonorHold = 1666, // 0x00000682
        HonorHold_2 = 1667, // 0x00000683
        Thrallmar = 1668, // 0x00000684
        Thrallmar_2 = 1669, // 0x00000685
        Thrallmar_3 = 1670, // 0x00000686
        HonorHold_3 = 1671, // 0x00000687
        TestFaction1 = 1672, // 0x00000688
        ToWoWFlag = 1673, // 0x00000689
        TestFaction4 = 1674, // 0x0000068A
        TestFaction3 = 1675, // 0x0000068B
        ToWoWFlagTriggerHordeDND = 1676, // 0x0000068C
        ToWoWFlagTriggerAllianceDND = 1677, // 0x0000068D
        Ethereum = 1678, // 0x0000068E
        Broken = 1679, // 0x0000068F
        Elemental_4 = 1680, // 0x00000690
        EarthElemental = 1681, // 0x00000691
        FightingRobots = 1682, // 0x00000692
        ActorGood = 1683, // 0x00000693
        ActorEvil = 1684, // 0x00000694
        StillpineFurbolg = 1685, // 0x00000695
        StillpineFurbolg_2 = 1686, // 0x00000696
        CrazedOwlkin = 1687, // 0x00000697
        ChessAlliance = 1688, // 0x00000698
        ChessHorde = 1689, // 0x00000699
        ChessAlliance_2 = 1690, // 0x0000069A
        ChessHorde_2 = 1691, // 0x0000069B
        MonsterSpar = 1692, // 0x0000069C
        MonsterSparBuddy = 1693, // 0x0000069D
        Exodar_8 = 1694, // 0x0000069E
        SilvermoonCity_7 = 1695, // 0x0000069F
        TheVioletEye = 1696, // 0x000006A0
        FelOrc_2 = 1697, // 0x000006A1
        Exodar_9 = 1698, // 0x000006A2
        Exodar_10 = 1699, // 0x000006A3
        Exodar_11 = 1700, // 0x000006A4
        Sunhawks = 1701, // 0x000006A5
        Sunhawks_2 = 1702, // 0x000006A6
        TrainingDummy_3 = 1703, // 0x000006A7
        FelOrc_3 = 1704, // 0x000006A8
        FelOrc_4 = 1705, // 0x000006A9
        FungalGiant = 1706, // 0x000006AA
        Sporeggar = 1707, // 0x000006AB
        Sporeggar_2 = 1708, // 0x000006AC
        Sporeggar_3 = 1709, // 0x000006AD
        CenarionExpedition_4 = 1710, // 0x000006AE
        MonsterPredator = 1711, // 0x000006AF
        MonsterPrey = 1712, // 0x000006B0
        MonsterPrey_2 = 1713, // 0x000006B1
        Sunhawks_3 = 1714, // 0x000006B2
        VoidAnomaly = 1715, // 0x000006B3
        HyjalDefenders = 1716, // 0x000006B4
        HyjalDefenders_2 = 1717, // 0x000006B5
        HyjalDefenders_3 = 1718, // 0x000006B6
        HyjalDefenders_4 = 1719, // 0x000006B7
        HyjalInvaders = 1720, // 0x000006B8
        Kurenai = 1721, // 0x000006B9
        Kurenai_2 = 1722, // 0x000006BA
        Kurenai_3 = 1723, // 0x000006BB
        Kurenai_4 = 1724, // 0x000006BC
        EarthenRing = 1725, // 0x000006BD
        EarthenRing_2 = 1726, // 0x000006BE
        EarthenRing_3 = 1727, // 0x000006BF
        CenarionExpedition_5 = 1728, // 0x000006C0
        Thrallmar_4 = 1729, // 0x000006C1
        TheConsortium = 1730, // 0x000006C2
        TheConsortium_2 = 1731, // 0x000006C3
        AllianceGeneric_8 = 1732, // 0x000006C4
        AllianceGeneric_9 = 1733, // 0x000006C5
        HordeGeneric_9 = 1734, // 0x000006C6
        HordeGeneric_10 = 1735, // 0x000006C7
        MonsterSparBuddy_2 = 1736, // 0x000006C8
        HonorHold_4 = 1737, // 0x000006C9
        Arakkoa = 1738, // 0x000006CA
        ZangarmarshBannerAlliance = 1739, // 0x000006CB
        ZangarmarshBannerHorde = 1740, // 0x000006CC
        TheShaTar = 1741, // 0x000006CD
        ZangarmarshBannerNeutral = 1742, // 0x000006CE
        TheAldor = 1743, // 0x000006CF
        TheScryers = 1744, // 0x000006D0
        SilvermoonCity_8 = 1745, // 0x000006D1
        TheScryers_2 = 1746, // 0x000006D2
        CavernsOfTimeThrall = 1747, // 0x000006D3
        CavernsOfTimeDurnholde = 1748, // 0x000006D4
        CavernsOfTimeSouthshoreGuards = 1749, // 0x000006D5
        ShadowCouncilCovert = 1750, // 0x000006D6
        Monster_7 = 1751, // 0x000006D7
        DarkPortalAttackerLegion = 1752, // 0x000006D8
        DarkPortalAttackerLegion_2 = 1753, // 0x000006D9
        DarkPortalAttackerLegion_3 = 1754, // 0x000006DA
        DarkPortalDefenderAlliance = 1755, // 0x000006DB
        DarkPortalDefenderAlliance_2 = 1756, // 0x000006DC
        DarkPortalDefenderAlliance_3 = 1757, // 0x000006DD
        DarkPortalDefenderHorde = 1758, // 0x000006DE
        DarkPortalDefenderHorde_2 = 1759, // 0x000006DF
        DarkPortalDefenderHorde_3 = 1760, // 0x000006E0
        InciterTrigger = 1761, // 0x000006E1
        InciterTrigger2 = 1762, // 0x000006E2
        InciterTrigger3 = 1763, // 0x000006E3
        InciterTrigger4 = 1764, // 0x000006E4
        InciterTrigger5 = 1765, // 0x000006E5
        ArgentDawn_5 = 1766, // 0x000006E6
        ArgentDawn_6 = 1767, // 0x000006E7
        Demon_3 = 1768, // 0x000006E8
        Demon_4 = 1769, // 0x000006E9
        ActorGood_2 = 1770, // 0x000006EA
        ActorEvil_2 = 1771, // 0x000006EB
        ManaCreature = 1772, // 0x000006EC
        KhadgarsServant = 1773, // 0x000006ED
        Friendly_3 = 1774, // 0x000006EE
        TheShaTar_2 = 1775, // 0x000006EF
        TheAldor_2 = 1776, // 0x000006F0
        TheAldor_3 = 1777, // 0x000006F1
        TheScaleOfTheSands = 1778, // 0x000006F2
        KeepersOfTime = 1779, // 0x000006F3
        BladespireClan = 1780, // 0x000006F4
        BloodmaulClan = 1781, // 0x000006F5
        BladespireClan_2 = 1782, // 0x000006F6
        BloodmaulClan_2 = 1783, // 0x000006F7
        BladespireClan_3 = 1784, // 0x000006F8
        BloodmaulClan_3 = 1785, // 0x000006F9
        Demon_5 = 1786, // 0x000006FA
        Monster_8 = 1787, // 0x000006FB
        TheConsortium_3 = 1788, // 0x000006FC
        Sunhawks_4 = 1789, // 0x000006FD
        BladespireClan_4 = 1790, // 0x000006FE
        BloodmaulClan_4 = 1791, // 0x000006FF
        FelOrc_5 = 1792, // 0x00000700
        Sunhawks_5 = 1793, // 0x00000701
        Protectorate = 1794, // 0x00000702
        Protectorate_2 = 1795, // 0x00000703
        Ethereum_2 = 1796, // 0x00000704
        Protectorate_3 = 1797, // 0x00000705
        ArcaneAnnihilatorDNR = 1798, // 0x00000706
        EthereumSparbuddy = 1799, // 0x00000707
        Ethereum_3 = 1800, // 0x00000708
        Horde = 1801, // 0x00000709
        Alliance = 1802, // 0x0000070A
        Ambient_3 = 1803, // 0x0000070B
        Ambient_4 = 1804, // 0x0000070C
        TheAldor_4 = 1805, // 0x0000070D
        Friendly_4 = 1806, // 0x0000070E
        Protectorate_4 = 1807, // 0x0000070F
        KirinVarBelmara = 1808, // 0x00000710
        KirinVarCohlien = 1809, // 0x00000711
        KirinVarDathric = 1810, // 0x00000712
        KirinVarLuminrath = 1811, // 0x00000713
        Friendly_5 = 1812, // 0x00000714
        ServantOfIllidan = 1813, // 0x00000715
        MonsterSparBuddy_3 = 1814, // 0x00000716
        BeastWolf_3 = 1815, // 0x00000717
        Friendly_6 = 1816, // 0x00000718
        LowerCity = 1818, // 0x0000071A
        AllianceGeneric_10 = 1819, // 0x0000071B
        AshtongueDeathsworn = 1820, // 0x0000071C
        SpiritsOfShadowmoon1 = 1821, // 0x0000071D
        SpiritsOfShadowmoon2 = 1822, // 0x0000071E
        Ethereum_4 = 1823, // 0x0000071F
        Netherwing = 1824, // 0x00000720
        Demon_6 = 1825, // 0x00000721
        ServantOfIllidan_2 = 1826, // 0x00000722
        Wyrmcult = 1827, // 0x00000723
        Treant = 1828, // 0x00000724
        LeotherasDemonI = 1829, // 0x00000725
        LeotherasDemonII = 1830, // 0x00000726
        LeotherasDemonIII = 1831, // 0x00000727
        LeotherasDemonIV = 1832, // 0x00000728
        LeotherasDemonV = 1833, // 0x00000729
        Azaloth = 1834, // 0x0000072A
        HordeGeneric_11 = 1835, // 0x0000072B
        TheConsortium_4 = 1836, // 0x0000072C
        Sporeggar_4 = 1837, // 0x0000072D
        TheScryers_3 = 1838, // 0x0000072E
        RockFlayer = 1839, // 0x0000072F
        FlayerHunter = 1840, // 0x00000730
        ShadowmoonShade = 1841, // 0x00000731
        LegionCommunicator = 1842, // 0x00000732
        ServantOfIllidan_3 = 1843, // 0x00000733
        TheAldor_5 = 1844, // 0x00000734
        TheScryers_4 = 1845, // 0x00000735
        RavenswoodAncients = 1846, // 0x00000736
        MonsterSpar_2 = 1847, // 0x00000737
        MonsterSparBuddy_4 = 1848, // 0x00000738
        ServantOfIllidan_4 = 1849, // 0x00000739
        Netherwing_2 = 1850, // 0x0000073A
        LowerCity_2 = 1851, // 0x0000073B
        ChessFriendlyToAllChess = 1852, // 0x0000073C
        ServantOfIllidan_5 = 1853, // 0x0000073D
        TheAldor_6 = 1854, // 0x0000073E
        TheScryers_5 = 1855, // 0x0000073F
        ShaTariSkyguard = 1856, // 0x00000740
        Friendly_7 = 1857, // 0x00000741
        AshtongueDeathsworn_2 = 1858, // 0x00000742
        Maiev = 1859, // 0x00000743
        SkettisShadowyArakkoa = 1860, // 0x00000744
        SkettisArakkoa = 1862, // 0x00000746
        OrcDragonmaw_2 = 1863, // 0x00000747
        DragonmawEnemy = 1864, // 0x00000748
        OrcDragonmaw_3 = 1865, // 0x00000749
        AshtongueDeathsworn_3 = 1866, // 0x0000074A
        Maiev_2 = 1867, // 0x0000074B
        MonsterSparBuddy_5 = 1868, // 0x0000074C
        Arakkoa_2 = 1869, // 0x0000074D
        ShaTariSkyguard_2 = 1870, // 0x0000074E
        SkettisArakkoa_2 = 1871, // 0x0000074F
        OgriLa = 1872, // 0x00000750
        RockFlayer_2 = 1873, // 0x00000751
        OgriLa_2 = 1874, // 0x00000752
        TheAldor_7 = 1875, // 0x00000753
        TheScryers_6 = 1876, // 0x00000754
        OrcDragonmaw_4 = 1877, // 0x00000755
        Frenzy = 1878, // 0x00000756
        SkyguardEnemy = 1879, // 0x00000757
        OrcDragonmaw_5 = 1880, // 0x00000758
        SkettisArakkoa_3 = 1881, // 0x00000759
        ServantOfIllidan_6 = 1882, // 0x0000075A
        TheramoreDeserter = 1883, // 0x0000075B
        Tuskarr = 1884, // 0x0000075C
        Vrykul = 1885, // 0x0000075D
        Creature_9 = 1886, // 0x0000075E
        Creature_10 = 1887, // 0x0000075F
        NorthseaPirates = 1888, // 0x00000760
        UNUSED = 1889, // 0x00000761
        TrollAmani = 1890, // 0x00000762
        ValianceExpedition = 1891, // 0x00000763
        ValianceExpedition_2 = 1892, // 0x00000764
        ValianceExpedition_3 = 1893, // 0x00000765
        Vrykul_2 = 1894, // 0x00000766
        Vrykul_3 = 1895, // 0x00000767
        DarkmoonFaire_2 = 1896, // 0x00000768
        TheHandOfVengeance = 1897, // 0x00000769
        ValianceExpedition_4 = 1898, // 0x0000076A
        ValianceExpedition_5 = 1899, // 0x0000076B
        TheHandOfVengeance_2 = 1900, // 0x0000076C
        HordeExpedition = 1901, // 0x0000076D
        ActorEvil_3 = 1902, // 0x0000076E
        ActorEvil_4 = 1904, // 0x00000770
        TamedPlaguehound = 1905, // 0x00000771
        SpottedGryphon = 1906, // 0x00000772
        TestFaction1_2 = 1907, // 0x00000773
        TestFaction1_3 = 1908, // 0x00000774
        BeastRaptor_2 = 1909, // 0x00000775
        VrykulAncientSpirit1 = 1910, // 0x00000776
        VrykulAncientSiprit2 = 1911, // 0x00000777
        VrykulAncientSiprit3 = 1912, // 0x00000778
        CTFFlagAlliance = 1913, // 0x00000779
        Vrykul_4 = 1914, // 0x0000077A
        Test = 1915, // 0x0000077B
        Maiev_3 = 1916, // 0x0000077C
        Creature_11 = 1917, // 0x0000077D
        HordeExpedition_2 = 1918, // 0x0000077E
        VrykulGladiator = 1919, // 0x0000077F
        ValgardeCombatant = 1920, // 0x00000780
        TheTaunka = 1921, // 0x00000781
        TheTaunka_2 = 1922, // 0x00000782
        TheTaunka_3 = 1923, // 0x00000783
        MonsterZoneForceReaction1 = 1924, // 0x00000784
        Monster_9 = 1925, // 0x00000785
        ExplorersLeague = 1926, // 0x00000786
        ExplorersLeague_2 = 1927, // 0x00000787
        TheHandOfVengeance_3 = 1928, // 0x00000788
        TheHandOfVengeance_4 = 1929, // 0x00000789
        RamRacingPowerupDND = 1930, // 0x0000078A
        RamRacingTrapDND = 1931, // 0x0000078B
        Elemental_5 = 1932, // 0x0000078C
        Friendly_8 = 1933, // 0x0000078D
        ActorGood_3 = 1934, // 0x0000078E
        ActorGood_4 = 1935, // 0x0000078F
        CraigsSquirrels = 1936, // 0x00000790
        CraigsSquirrels_2 = 1937, // 0x00000791
        CraigsSquirrels_3 = 1938, // 0x00000792
        CraigsSquirrels_4 = 1939, // 0x00000793
        CraigsSquirrels_5 = 1940, // 0x00000794
        CraigsSquirrels_6 = 1941, // 0x00000795
        CraigsSquirrels_7 = 1942, // 0x00000796
        CraigsSquirrels_8 = 1943, // 0x00000797
        CraigsSquirrels_9 = 1944, // 0x00000798
        CraigsSquirrels_10 = 1945, // 0x00000799
        CraigsSquirrels_11 = 1947, // 0x0000079B
        Blue_2 = 1948, // 0x0000079C
        TheKaluAk = 1949, // 0x0000079D
        TheKaluAk_2 = 1950, // 0x0000079E
        Darnassus_8 = 1951, // 0x0000079F
        HolidayWaterBarrel = 1952, // 0x000007A0
        MonsterPredator_2 = 1953, // 0x000007A1
        IronDwarves = 1954, // 0x000007A2
        IronDwarves_2 = 1955, // 0x000007A3
        ShatteredSunOffensive = 1956, // 0x000007A4
        ShatteredSunOffensive_2 = 1957, // 0x000007A5
        ActorEvil_5 = 1958, // 0x000007A6
        ActorEvil_6 = 1959, // 0x000007A7
        ShatteredSunOffensive_3 = 1960, // 0x000007A8
        FightingVanityPet = 1961, // 0x000007A9
        UndeadScourge_5 = 1962, // 0x000007AA
        Demon_7 = 1963, // 0x000007AB
        UndeadScourge_6 = 1964, // 0x000007AC
        MonsterSpar_3 = 1965, // 0x000007AD
        Murloc_2 = 1966, // 0x000007AE
        ShatteredSunOffensive_4 = 1967, // 0x000007AF
        MurlocWinterfin = 1968, // 0x000007B0
        Murloc_3 = 1969, // 0x000007B1
        Monster_10 = 1970, // 0x000007B2
        FriendlyForceReaction = 1971, // 0x000007B3
        ObjectForceReaction = 1972, // 0x000007B4
        ValianceExpedition_6 = 1973, // 0x000007B5
        ValianceExpedition_7 = 1974, // 0x000007B6
        UndeadScourge_7 = 1975, // 0x000007B7
        ValianceExpedition_8 = 1976, // 0x000007B8
        ValianceExpedition_9 = 1977, // 0x000007B9
        WarsongOffensive = 1978, // 0x000007BA
        WarsongOffensive_2 = 1979, // 0x000007BB
        WarsongOffensive_3 = 1980, // 0x000007BC
        WarsongOffensive_4 = 1981, // 0x000007BD
        UndeadScourge_8 = 1982, // 0x000007BE
        MonsterSpar_4 = 1983, // 0x000007BF
        MonsterSparBuddy_6 = 1984, // 0x000007C0
        Monster_11 = 1985, // 0x000007C1
        Escortee_11 = 1986, // 0x000007C2
        CenarionExpedition_6 = 1987, // 0x000007C3
        UndeadScourge_9 = 1988, // 0x000007C4
        Poacher = 1989, // 0x000007C5
        Ambient_5 = 1990, // 0x000007C6
        UndeadScourge_10 = 1991, // 0x000007C7
        Monster_12 = 1992, // 0x000007C8
        MonsterSpar_5 = 1993, // 0x000007C9
        MonsterSparBuddy_7 = 1994, // 0x000007CA
        CTFFlagAlliance_2 = 1995, // 0x000007CB
        CTFFlagAlliance_3 = 1997, // 0x000007CD
        HolidayMonster = 1998, // 0x000007CE
        MonsterPrey_3 = 1999, // 0x000007CF
        MonsterPrey_4 = 2000, // 0x000007D0
        FurbolgRedfang = 2001, // 0x000007D1
        FurbolgFrostpaw = 2003, // 0x000007D3
        ValianceExpedition_10 = 2004, // 0x000007D4
        UndeadScourge_11 = 2005, // 0x000007D5
        KirinTor = 2006, // 0x000007D6
        KirinTor_2 = 2007, // 0x000007D7
        KirinTor_3 = 2008, // 0x000007D8
        KirinTor_4 = 2009, // 0x000007D9
        TheWyrmrestAccord = 2010, // 0x000007DA
        TheWyrmrestAccord_2 = 2011, // 0x000007DB
        TheWyrmrestAccord_3 = 2012, // 0x000007DC
        TheWyrmrestAccord_4 = 2013, // 0x000007DD
        AzjolNerub = 2014, // 0x000007DE
        AzjolNerub_2 = 2016, // 0x000007E0
        AzjolNerub_3 = 2017, // 0x000007E1
        UndeadScourge_12 = 2018, // 0x000007E2
        TheTaunka_4 = 2019, // 0x000007E3
        WarsongOffensive_5 = 2020, // 0x000007E4
        REUSE = 2021, // 0x000007E5
        Monster_13 = 2022, // 0x000007E6
        ScourgeInvaders_3 = 2023, // 0x000007E7
        TheHandOfVengeance_5 = 2024, // 0x000007E8
        TheSilverCovenant = 2025, // 0x000007E9
        TheSilverCovenant_2 = 2026, // 0x000007EA
        TheSilverCovenant_3 = 2027, // 0x000007EB
        Ambient_6 = 2028, // 0x000007EC
        MonsterPredator_3 = 2029, // 0x000007ED
        MonsterPredator_4 = 2030, // 0x000007EE
        HordeGeneric_12 = 2031, // 0x000007EF
        GrizzlyHillsTrapper = 2032, // 0x000007F0
        Monster_14 = 2033, // 0x000007F1
        WarsongOffensive_6 = 2034, // 0x000007F2
        UndeadScourge_13 = 2035, // 0x000007F3
        Friendly_9 = 2036, // 0x000007F4
        ValianceExpedition_11 = 2037, // 0x000007F5
        Ambient_7 = 2038, // 0x000007F6
        Monster_15 = 2039, // 0x000007F7
        ValianceExpedition_12 = 2040, // 0x000007F8
        TheWyrmrestAccord_5 = 2041, // 0x000007F9
        UndeadScourge_14 = 2042, // 0x000007FA
        UndeadScourge_15 = 2043, // 0x000007FB
        ValianceExpedition_13 = 2044, // 0x000007FC
        WarsongOffensive_7 = 2045, // 0x000007FD
        Escortee_12 = 2046, // 0x000007FE
        TheKaluAk_3 = 2047, // 0x000007FF
        ScourgeInvaders_4 = 2048, // 0x00000800
        ScourgeInvaders_5 = 2049, // 0x00000801
        KnightsOfTheEbonBlade = 2050, // 0x00000802
        KnightsOfTheEbonBlade_2 = 2051, // 0x00000803
        WrathgateScourge = 2052, // 0x00000804
        WrathgateAlliance = 2053, // 0x00000805
        WrathgateHorde = 2054, // 0x00000806
        MonsterSpar_6 = 2055, // 0x00000807
        MonsterSparBuddy_8 = 2056, // 0x00000808
        MonsterZoneForceReaction2 = 2057, // 0x00000809
        CTFFlagHorde = 2058, // 0x0000080A
        CTFFlagNeutral = 2059, // 0x0000080B
        FrenzyheartTribe = 2060, // 0x0000080C
        FrenzyheartTribe_2 = 2061, // 0x0000080D
        FrenzyheartTribe_3 = 2062, // 0x0000080E
        TheOracles = 2063, // 0x0000080F
        TheOracles_2 = 2064, // 0x00000810
        TheOracles_3 = 2065, // 0x00000811
        TheOracles_4 = 2066, // 0x00000812
        TheWyrmrestAccord_6 = 2067, // 0x00000813
        UndeadScourge_16 = 2068, // 0x00000814
        TrollDrakkari = 2069, // 0x00000815
        ArgentCrusade = 2070, // 0x00000816
        ArgentCrusade_2 = 2071, // 0x00000817
        ArgentCrusade_3 = 2072, // 0x00000818
        ArgentCrusade_4 = 2073, // 0x00000819
        CavernsOfTimeDurnholde_2 = 2074, // 0x0000081A
        CoTScourge = 2075, // 0x0000081B
        CoTArthas = 2076, // 0x0000081C
        CoTArthas_2 = 2077, // 0x0000081D
        CoTStratholmeCitizen = 2078, // 0x0000081E
        CoTArthas_3 = 2079, // 0x0000081F
        UndeadScourge_17 = 2080, // 0x00000820
        Freya = 2081, // 0x00000821
        UndeadScourge_18 = 2082, // 0x00000822
        UndeadScourge_19 = 2083, // 0x00000823
        UndeadScourge_20 = 2084, // 0x00000824
        UndeadScourge_21 = 2085, // 0x00000825
        ArgentDawn_7 = 2086, // 0x00000826
        ArgentDawn_8 = 2087, // 0x00000827
        ActorEvil_7 = 2088, // 0x00000828
        ScarletCrusade_3 = 2089, // 0x00000829
        MountTaxiAlliance = 2090, // 0x0000082A
        MountTaxiHorde = 2091, // 0x0000082B
        MountTaxiNeutral = 2092, // 0x0000082C
        UndeadScourge_22 = 2093, // 0x0000082D
        UndeadScourge_23 = 2094, // 0x0000082E
        ScarletCrusade_4 = 2095, // 0x0000082F
        ScarletCrusade_5 = 2096, // 0x00000830
        UndeadScourge_24 = 2097, // 0x00000831
        ElementalAir = 2098, // 0x00000832
        ElementalWater = 2099, // 0x00000833
        UndeadScourge_25 = 2100, // 0x00000834
        ActorEvil_8 = 2101, // 0x00000835
        ActorEvil_9 = 2102, // 0x00000836
        ScarletCrusade_6 = 2103, // 0x00000837
        MonsterSpar_7 = 2104, // 0x00000838
        MonsterSparBuddy_9 = 2105, // 0x00000839
        Ambient_8 = 2106, // 0x0000083A
        TheSonsOfHodir = 2107, // 0x0000083B
        IronGiants = 2108, // 0x0000083C
        FrostVrykul = 2109, // 0x0000083D
        Friendly_10 = 2110, // 0x0000083E
        Monster_16 = 2111, // 0x0000083F
        TheSonsOfHodir_2 = 2112, // 0x00000840
        FrostVrykul_2 = 2113, // 0x00000841
        Vrykul_5 = 2114, // 0x00000842
        ActorGood_5 = 2115, // 0x00000843
        Vrykul_6 = 2116, // 0x00000844
        ActorGood_6 = 2117, // 0x00000845
        Earthen = 2118, // 0x00000846
        MonsterReferee = 2119, // 0x00000847
        MonsterReferee_2 = 2120, // 0x00000848
        TheSunreavers = 2121, // 0x00000849
        TheSunreavers_2 = 2122, // 0x0000084A
        TheSunreavers_3 = 2123, // 0x0000084B
        Monster_17 = 2124, // 0x0000084C
        FrostVrykul_3 = 2125, // 0x0000084D
        FrostVrykul_4 = 2126, // 0x0000084E
        Ambient_9 = 2127, // 0x0000084F
        Hyldsmeet = 2128, // 0x00000850
        TheSunreavers_4 = 2129, // 0x00000851
        TheSilverCovenant_4 = 2130, // 0x00000852
        ArgentCrusade_5 = 2131, // 0x00000853
        WarsongOffensive_8 = 2132, // 0x00000854
        FrostVrykul_5 = 2133, // 0x00000855
        ArgentCrusade_6 = 2134, // 0x00000856
        Friendly_11 = 2135, // 0x00000857
        Ambient_10 = 2136, // 0x00000858
        Friendly_12 = 2137, // 0x00000859
        ArgentCrusade_7 = 2138, // 0x0000085A
        ScourgeInvaders_6 = 2139, // 0x0000085B
        Friendly_13 = 2140, // 0x0000085C
        Friendly_14 = 2141, // 0x0000085D
        Alliance_2 = 2142, // 0x0000085E
        ValianceExpedition_14 = 2143, // 0x0000085F
        KnightsOfTheEbonBlade_3 = 2144, // 0x00000860
        ScourgeInvaders_7 = 2145, // 0x00000861
        TheKaluAk_4 = 2148, // 0x00000864
        MonsterSparBuddy_10 = 2150, // 0x00000866
        Ironforge_6 = 2155, // 0x0000086B
        MonsterPredator_5 = 2156, // 0x0000086C
        ActorGood_7 = 2176, // 0x00000880
        ActorGood_8 = 2178, // 0x00000882
        HatesEverything = 2189, // 0x0000088D
        HatesEverything_2 = 2190, // 0x0000088E
        HatesEverything_3 = 2191, // 0x0000088F
        UndeadScourge_26 = 2209, // 0x000008A1
        SilvermoonCity_9 = 2210, // 0x000008A2
        UndeadScourge_27 = 2212, // 0x000008A4
        KnightsOfTheEbonBlade_4 = 2214, // 0x000008A6
        TheAshenVerdict = 2216, // 0x000008A8
        TheAshenVerdict_2 = 2217, // 0x000008A9
        TheAshenVerdict_3 = 2218, // 0x000008AA
        TheAshenVerdict_4 = 2219, // 0x000008AB
        KnightsOfTheEbonBlade_5 = 2226, // 0x000008B2
        ArgentCrusade_8 = 2230, // 0x000008B6
        CTFFlagHorde2 = 2235, // 0x000008BB
        CTFFlagAlliance2 = 2236, // 0x000008BC
        End = 2237, // 0x000008BD
    }
}