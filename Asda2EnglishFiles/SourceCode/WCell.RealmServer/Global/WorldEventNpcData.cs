using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Holds all information regarding a change of model
    /// or equipment for a WorldEvent
    /// </summary>
    public class WorldEventNpcData : IDataHolder
    {
        /// <summary>Spawn id of the object</summary>
        public uint Guid;

        public NPCId EntryId;
        [NotPersistent] public NPCId OriginalEntryId;
        public uint ModelId;
        public uint EquipmentId;
        [NotPersistent] public uint OriginalEquipmentId;
        public SpellId SpellIdToCastAtStart;
        public SpellId SpellIdToCastAtEnd;

        /// <summary>ID of the world event relating to this entry</summary>
        public uint EventId;

        public void FinalizeDataHolder()
        {
            WorldEvent worldEvent = WorldEventMgr.GetEvent(this.EventId);
            if (worldEvent == null)
                return;
            worldEvent.ModelEquips.Add(this);
        }
    }
}