using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 2: An object that can start or end a quest.</summary>
    public class QuestGiverHandler : GameObjectHandler
    {
        public override bool Use(Character user)
        {
            GOQuestGiverEntry entry = (GOQuestGiverEntry) this.m_go.Entry;
            return true;
        }
    }
}