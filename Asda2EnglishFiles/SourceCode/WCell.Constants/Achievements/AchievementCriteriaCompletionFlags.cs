using System;

namespace WCell.Constants.Achievements
{
    [Flags]
    public enum AchievementCriteriaCompletionFlags : uint
    {
        ShowProgressBar = 1,
        FlagHideCriteria = 2,
        FailAchievement = 4,
        ResetOnStart = 8,
        IsDate = 16, // 0x00000010
        IsMoney = 32, // 0x00000020
    }
}