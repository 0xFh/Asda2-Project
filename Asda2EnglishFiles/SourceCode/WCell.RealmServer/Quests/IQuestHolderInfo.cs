using System.Collections.Generic;
using WCell.Constants.Quests;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;

namespace WCell.RealmServer.Quests
{
    public interface IQuestHolderInfo
    {
        QuestStatus GetHighestQuestGiverStatus(Character chr);

        List<QuestTemplate> GetAvailableQuests(Character chr);

        List<QuestMenuItem> GetQuestMenuItems(Character chr);
    }
}