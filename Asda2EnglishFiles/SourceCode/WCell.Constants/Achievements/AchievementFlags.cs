using System;

namespace WCell.Constants.Achievements
{
    [Flags]
    public enum AchievementFlags : uint
    {
        Counter = 1,
        Unk2 = 2,
        StoreMaxValue = 4,
        Summ = 8,
        MaxUsed = 16, // 0x00000010
        ReqCount = 32, // 0x00000020
        Average = 64, // 0x00000040
        Bar = 128, // 0x00000080
        RealmFirstReach = 256, // 0x00000100
        RealmFirstKill = 512, // 0x00000200
    }
}