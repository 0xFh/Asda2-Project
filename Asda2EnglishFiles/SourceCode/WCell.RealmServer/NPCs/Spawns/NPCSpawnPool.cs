using System;
using System.Collections.Generic;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spawns;

namespace WCell.RealmServer.NPCs.Spawns
{
    [Serializable]
    public class NPCSpawnPool : SpawnPool<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>
    {
        public NPCSpawnPool(Map map, NPCSpawnPoolTemplate templ)
            : base(map, templ)
        {
            this.AIGroup = new AIGroup();
        }

        public AIGroup AIGroup { get; private set; }

        public override IList<NPC> SpawnedObjects
        {
            get { return (IList<NPC>) this.AIGroup; }
        }
    }
}