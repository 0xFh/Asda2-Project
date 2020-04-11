namespace WCell.Constants.Quests
{
    public static class QuestStatusHelper
    {
        public static bool CanStartOrFinish(this QuestStatus status)
        {
            return status >= QuestStatus.RepeateableCompletable;
        }
    }
}