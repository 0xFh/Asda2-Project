using System;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spawns;

namespace WCell.RealmServer.NPCs.Spawns
{
    [Serializable]
    public class NPCSpawnPoint : SpawnPoint<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>,
        IWorldLocation, IHasPosition
    {
    }
}