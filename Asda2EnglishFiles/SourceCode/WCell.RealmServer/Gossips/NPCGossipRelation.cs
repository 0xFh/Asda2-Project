using WCell.RealmServer.Content;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Data;

namespace WCell.RealmServer.Gossips
{
    public class NPCGossipRelation : IDataHolder
    {
        public uint NPCSpawnId;
        public uint GossipId;

        public uint GetId()
        {
            return this.NPCSpawnId;
        }

        public DataHolderState DataHolderState { get; set; }

        public void FinalizeDataHolder()
        {
            NPCSpawnEntry spawnEntry = NPCMgr.GetSpawnEntry(this.NPCSpawnId);
            IGossipEntry entry1 = GossipMgr.GetEntry(this.GossipId);
            if (spawnEntry == null)
                ContentMgr.OnInvalidDBData("{0} refers to invalid spawn id: {1}", (object) this.GetType(),
                    (object) this);
            else if (entry1 == null)
            {
                ContentMgr.OnInvalidDBData("{0} has invalid GossipId: {1}", (object) this.GetType(), (object) this);
            }
            else
            {
                NPCEntry entry2 = spawnEntry.Entry;
                if (spawnEntry.DefaultGossip == null)
                    spawnEntry.DefaultGossip = new GossipMenu(entry1);
                else
                    spawnEntry.DefaultGossip.GossipEntry = entry1;
            }
        }

        public override string ToString()
        {
            return string.Format("NPC: {0} <-> Gossip id: {1}", (object) this.NPCSpawnId, (object) this.GossipId);
        }
    }
}