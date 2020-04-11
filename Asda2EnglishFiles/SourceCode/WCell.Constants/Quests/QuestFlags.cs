using System;

namespace WCell.Constants.Quests
{
    [Flags]
    public enum QuestFlags : uint
    {
        None = 0,
        Deliver = 1,
        Escort = 2,
        Explore = 4,
        Sharable = 8,
        Exploration = 16, // 0x00000010
        Timed = 32, // 0x00000020
        Raid = 64, // 0x00000040
        TBCOnly = 128, // 0x00000080
        DeliverMore = 256, // 0x00000100
        HiddenRewards = 512, // 0x00000200
        Unknown4 = 1024, // 0x00000400
        TBCRaces = 2048, // 0x00000800
        Daily = 4096, // 0x00001000
        AutoAccept = 524288, // 0x00080000
    }
}