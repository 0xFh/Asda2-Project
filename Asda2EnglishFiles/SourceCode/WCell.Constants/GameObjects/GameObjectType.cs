namespace WCell.Constants.GameObjects
{
    public enum GameObjectType : uint
    {
        Door = 0,
        Button = 1,
        QuestGiver = 2,
        Chest = 3,
        Binder = 4,
        Generic = 5,
        Trap = 6,
        Chair = 7,
        SpellFocus = 8,
        Text = 9,
        Goober = 10, // 0x0000000A
        Transport = 11, // 0x0000000B
        AreaDamage = 12, // 0x0000000C
        Camera = 13, // 0x0000000D
        MapObject = 14, // 0x0000000E
        MOTransport = 15, // 0x0000000F
        DuelFlag = 16, // 0x00000010
        FishingNode = 17, // 0x00000011
        SummoningRitual = 18, // 0x00000012
        Mailbox = 19, // 0x00000013
        DONOTUSE = 20, // 0x00000014
        GuardPost = 21, // 0x00000015
        SpellCaster = 22, // 0x00000016
        MeetingStone = 23, // 0x00000017
        FlagStand = 24, // 0x00000018
        FishingHole = 25, // 0x00000019
        FlagDrop = 26, // 0x0000001A
        MiniGame = 27, // 0x0000001B
        LotteryKiosk = 28, // 0x0000001C
        CapturePoint = 29, // 0x0000001D
        AuraGenerator = 30, // 0x0000001E
        DungeonDifficulty = 31, // 0x0000001F
        BarberChair = 32, // 0x00000020
        DestructibleBuilding = 33, // 0x00000021
        GuildBank = 34, // 0x00000022
        TrapDoor = 35, // 0x00000023
        Custom = 100, // 0x00000064
        End = 101, // 0x00000065
    }
}