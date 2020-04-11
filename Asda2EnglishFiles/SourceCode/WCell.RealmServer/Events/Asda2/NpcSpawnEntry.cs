using System;
using System.Collections.Generic;
using WCell.RealmServer.AI;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Events.Asda2
{
    [Serializable]
    public class NpcSpawnEntry
    {
        public List<Vector3> MovingPoints = new List<Vector3>();

        public NPCEntry NpcEntry { get; set; }

        public int TimeToSpawnMillis { get; set; }

        public BrainState BrainState { get; set; }

        public NpcSpawnEntry AddMovingPoint(int x, int y)
        {
            this.MovingPoints.Add(new Vector3((float) x, (float) y));
            return this;
        }
    }
}