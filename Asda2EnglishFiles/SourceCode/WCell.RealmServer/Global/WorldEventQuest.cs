using WCell.Util.Data;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Holds all information regarding a quest
    /// involved in a WorldEvent
    /// </summary>
    public class WorldEventQuest : IDataHolder
    {
        /// <summary>Id of the quest</summary>
        public uint QuestId;

        /// <summary>ID of the world event relating to this entry</summary>
        public uint EventId;

        public void FinalizeDataHolder()
        {
            WorldEventMgr.WorldEventQuests.Add(this);
            WorldEvent worldEvent = WorldEventMgr.GetEvent(this.EventId);
            if (worldEvent == null)
                return;
            worldEvent.QuestIds.Add(this.QuestId);
        }
    }
}