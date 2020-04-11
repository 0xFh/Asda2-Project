namespace WCell.Constants.Misc
{
    /// <summary>Message Types</summary>
    public enum ChatMsgType
    {
        Addon = -1,
        System = 0,
        Say = 1,
        Party = 2,
        Raid = 3,
        Guild = 4,
        Officer = 5,
        Yell = 6,
        Whisper = 7,
        WhisperInform = 8,
        MsgReply = 9,
        Emote = 10, // 0x0000000A
        MonsterSay = 12, // 0x0000000C
        MonsterParty = 13, // 0x0000000D
        MonsterYell = 14, // 0x0000000E
        MonsterWhisper = 15, // 0x0000000F
        MonsterEmote = 16, // 0x00000010
        Channel = 17, // 0x00000011
        AFK = 23, // 0x00000017
        DND = 24, // 0x00000018
        Ignored = 25, // 0x00000019
        CombatXPGain = 33, // 0x00000021
        BGSystemNeutral = 36, // 0x00000024
        BGSystemAlliance = 37, // 0x00000025
        BGSystemHorde = 38, // 0x00000026
        RaidLeader = 39, // 0x00000027
        RaidWarn = 40, // 0x00000028
        RaidBossEmote = 41, // 0x00000029
        RaidBossWhisper = 42, // 0x0000002A
        Filtered = 43, // 0x0000002B
        Battleground = 44, // 0x0000002C
        BattlegroundLeader = 45, // 0x0000002D
        Restricted = 46, // 0x0000002E
        Battlenet = 47, // 0x0000002F
        Achievment = 48, // 0x00000030
        ArenaPoints = 49, // 0x00000031
        PartyLeader = 50, // 0x00000032
        End = 51, // 0x00000033
    }
}