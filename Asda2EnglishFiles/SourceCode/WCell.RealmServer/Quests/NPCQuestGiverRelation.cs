using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Quests
{
    public class NPCQuestGiverRelation : QuestGiverRelation
    {
        public override ObjectTemplate ObjectTemplate
        {
            get { return (ObjectTemplate) NPCMgr.GetEntry(this.QuestGiverId); }
        }
    }
}