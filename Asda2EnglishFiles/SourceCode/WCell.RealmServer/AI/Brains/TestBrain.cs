using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;

namespace WCell.RealmServer.AI.Brains
{
    public class TestBrain : BaseBrain
    {
        private bool m_moveForward = true;
        private LinkedList<WaypointEntry> m_waypoints;
        private LinkedListNode<WaypointEntry> m_cur;

        public TestBrain(NPC owner)
            : base((Unit) owner)
        {
            NPCSpawnPoint spawnPoint = owner.SpawnPoint;
            if (spawnPoint == null)
                return;
            NPCSpawnEntry spawnEntry = spawnPoint.SpawnEntry;
            if (spawnEntry == null)
                return;
            this.m_waypoints = spawnEntry.Waypoints;
        }

        public void GoToNextWP()
        {
            if (this.m_waypoints == null || this.m_waypoints.Count < 2)
                return;
            this.m_cur = this.m_cur != null ? this.m_cur.Next : this.m_waypoints.First;
            if (!this.m_moveForward)
                return;
            this.m_owner.Movement.MoveTo(this.m_cur.Value.Position, false);
            if (this.m_cur.Next != null)
                return;
            this.m_moveForward = false;
        }

        public int WaypointsCount()
        {
            return this.m_waypoints.Count;
        }

        public void GoToWaypoint(WaypointEntry waypointEntry)
        {
            this.m_owner.Movement.MoveTo(waypointEntry.Position, false);
        }

        public void EnqueueAllWaypoints()
        {
            foreach (WaypointEntry waypoint in this.m_waypoints)
                this.GoToWaypoint(waypoint);
        }
    }
}