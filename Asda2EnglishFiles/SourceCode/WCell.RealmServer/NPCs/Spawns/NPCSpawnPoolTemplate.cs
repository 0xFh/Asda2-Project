using System;
using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spawns;

namespace WCell.RealmServer.NPCs.Spawns
{
    [Serializable]
    public class
        NPCSpawnPoolTemplate : SpawnPoolTemplate<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>
    {
        public NPCSpawnPoolTemplate()
            : this(0U, 0)
        {
        }

        /// <summary>Constructor for custom pools</summary>
        public NPCSpawnPoolTemplate(int maxSpawnAmount)
            : this(0U, maxSpawnAmount)
        {
        }

        /// <summary>Constructor for custom pools</summary>
        public NPCSpawnPoolTemplate(NPCSpawnEntry entry, int maxSpawnAmount = 0)
            : this(maxSpawnAmount)
        {
            this.AddEntry(entry);
        }

        internal NPCSpawnPoolTemplate(uint id, int maxSpawnAmount)
            : base(id, maxSpawnAmount)
        {
        }

        internal NPCSpawnPoolTemplate(SpawnPoolTemplateEntry entry)
            : base(entry)
        {
        }

        public override List<NPCSpawnPoolTemplate> PoolTemplatesOnSameMap
        {
            get { return NPCMgr.GetOrCreateSpawnPoolTemplatesByMap(this.MapId); }
        }
    }
}