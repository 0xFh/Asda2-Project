using System;

namespace WCell.Constants.Quests
{
    [Flags]
    public enum QuestSpecialFlags : uint
    {
        NoExtraRequirements = 0,
        MakeRepeateable = 1,
        EventCompletable = 2,
        RepeateableEventCompleteable = EventCompletable | MakeRepeateable, // 0x00000003
    }
}