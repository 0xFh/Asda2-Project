namespace WCell.Constants.Quests
{
    public enum QuestInvalidReason : byte
    {
        NoRequirements = 0,
        LowLevel = 1,
        WrongClass = 5,
        WrongRace = 6,
        AlreadyCompleted = 7,
        AlreadyOnTimedQuest = 12, // 0x0C
        AlreadyHave = 13, // 0x0D
        NoExpansionAccount = 16, // 0x10
        NoRequiredItems = 21, // 0x15
        NotEnoughMoney = 23, // 0x17
        TooManyDailys = 26, // 0x1A
        Tired = 27, // 0x1B
        Ok = 255, // 0xFF
    }
}