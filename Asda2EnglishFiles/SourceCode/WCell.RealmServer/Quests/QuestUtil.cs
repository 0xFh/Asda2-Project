using WCell.Constants.Quests;

namespace WCell.RealmServer.Quests
{
    public static class QuestUtil
    {
        public static bool CanFinish(this QuestStatus status)
        {
            if (status != QuestStatus.Completable && status != QuestStatus.CompletableNoMinimap)
                return status == QuestStatus.RepeateableCompletable;
            return true;
        }

        public static bool IsAvailable(this QuestStatus status)
        {
            if (status != QuestStatus.NotAvailable)
                return status != QuestStatus.TooHighLevel;
            return false;
        }
    }
}