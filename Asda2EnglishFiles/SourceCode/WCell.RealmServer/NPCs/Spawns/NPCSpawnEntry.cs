using System;
using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.Terrain;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Spawns;
using WCell.RealmServer.Waypoints;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs.Spawns
{
    /// <summary>Spawn-information for NPCs</summary>
    [DataHolder]
    [Serializable]
    public class NPCSpawnEntry : SpawnEntry<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>,
        INPCDataHolder, IDataHolder
    {
        [NotPersistent] public readonly LinkedList<WaypointEntry> Waypoints = new LinkedList<WaypointEntry>();
        public NPCId EntryId;
        public AIMotionGenerationType MoveType;
        public float RespawnRadius;
        public bool IsDead;
        public uint DisplayIdOverride;
        public uint EquipmentId;
        [NotPersistent] public NPCEquipmentEntry Equipment;
        [NotPersistent] public GossipMenu DefaultGossip;

        /// <summary>
        /// Called when a new NPC of this Spawn has been added to the world (also called on Teleport to another Map).
        /// </summary>
        public event Action<NPC> Spawned;

        public NPCSpawnEntry()
        {
        }

        public NPCSpawnEntry(NPCId entryId, MapId map, Vector3 pos)
        {
            this.EntryId = entryId;
            this.MapId = map;
            this.Position = pos;
        }

        [NotPersistent] public NPCEntry Entry { get; set; }

        public NPCAddonData AddonData { get; set; }

        public NPC SpawnObject(Map map)
        {
            NPC npc = this.Entry.Create(map.DifficultyIndex);
            map.AddObjectNow((WorldObject) npc, this.Position);
            return npc;
        }

        public override NPC SpawnObject(NPCSpawnPoint point)
        {
            NPC npc = this.Entry.Create(point);
            point.Map.AddObjectNow((WorldObject) npc, this.Position);
            return npc;
        }

        /// <summary>Finalize this NPCSpawnEntry</summary>
        /// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom NPCSpawnEntry that share a pool)</param>
        public void FinalizeDataHolder()
        {
            this.FinalizeDataHolder(true);
        }

        private void AddToPoolTemplate()
        {
            this.m_PoolTemplate = NPCMgr.GetOrCreateSpawnPoolTemplate(this.PoolId);
            this.m_PoolTemplate.AddEntry(this);
        }

        /// <summary>Finalize this NPCSpawnEntry</summary>
        /// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom NPCSpawnEntry that share a pool)</param>
        public override void FinalizeDataHolder(bool addToPool)
        {
            if (this.Entry == null)
            {
                this.Entry = NPCMgr.GetEntry(this.EntryId);
                if (this.Entry == null)
                {
                    ContentMgr.OnInvalidDBData("{0} had an invalid EntryId.", (object) this);
                    return;
                }
            }

            if (this.EquipmentId != 0U)
                this.Equipment = NPCMgr.GetEquipment(this.EquipmentId);
            if (this.DisplayIdOverride == 16777215U)
                this.DisplayIdOverride = 0U;
            base.FinalizeDataHolder(addToPool);
            if (this.MapId != MapId.End)
            {
                this.Entry.SpawnEntries.Add(this);
                ArrayUtil.Set<NPCSpawnEntry>(ref NPCMgr.SpawnEntries, this.SpawnId, this);
                if (addToPool)
                    this.AddToPoolTemplate();
            }

            if (this._eventId != (short) 0)
            {
                uint id = (uint) Math.Abs(this._eventId);
                WorldEvent worldEvent = WorldEventMgr.GetEvent(id);
                if (worldEvent != null)
                {
                    WorldEventNPC worldEventNpc = new WorldEventNPC()
                    {
                        _eventId = (int) this._eventId,
                        EventId = id,
                        Guid = this.SpawnId,
                        Spawn = this._eventId > (short) 0
                    };
                    worldEvent.NPCSpawns.Add(worldEventNpc);
                }

                this.EventId = id;
            }

            foreach (NPCSpawnTypeHandler spawnTypeHandler in this.Entry.SpawnTypeHandlers)
            {
                if (spawnTypeHandler != null)
                    spawnTypeHandler(this);
            }
        }

        internal void NotifySpawned(NPC npc)
        {
            Action<NPC> spawned = this.Spawned;
            if (spawned == null)
                return;
            spawned(npc);
        }

        /// <summary>Whether this SpawnEntry has fixed Waypoints from DB</summary>
        [NotPersistent]
        public bool HasDefaultWaypoints { get; set; }

        public void RecreateRandomWaypoints()
        {
            this.Waypoints.Clear();
            this.CreateRandomWaypoints();
        }

        public void CreateRandomWaypoints()
        {
            ITerrain terrain = TerrainMgr.GetTerrain(this.MapId);
            if (terrain == null)
                return;
            this.AddWaypoints(
                new RandomWaypointGenerator().GenerateWaypoints(terrain, this.Position, this.RespawnRadius));
            this.Waypoints.Last.Value.WaitTime = (uint) Utility.Random(2000, 7000);
        }

        public int WaypointCount
        {
            get { return this.Waypoints.Count; }
        }

        /// <summary>Creates a Waypoint but does not add it</summary>
        public WaypointEntry CreateWaypoint(Vector3 pos, float orientation)
        {
            LinkedListNode<WaypointEntry> last = this.Waypoints.Last;
            WaypointEntry waypointEntry;
            if (last != null)
                waypointEntry = new WaypointEntry()
                {
                    Id = last.Value.Id + 1U,
                    SpawnEntry = this
                };
            else
                waypointEntry = new WaypointEntry()
                {
                    Id = 1U,
                    SpawnEntry = this
                };
            waypointEntry.Position = pos;
            waypointEntry.Orientation = orientation;
            return waypointEntry;
        }

        /// <summary>Adds the given positions as WPs</summary>
        /// <param name="wps"></param>
        public void AddWaypoints(Vector3[] wps)
        {
            if (wps.Length < 1)
                throw new ArgumentException("wps are empty.");
            int length = wps.Length;
            Vector3 v;
            if (this.Waypoints.Count > 0)
            {
                v = this.Waypoints.First.Value.Position;
                WaypointEntry waypointEntry = this.Waypoints.Last.Value;
                waypointEntry.Orientation = waypointEntry.Position.GetAngleTowards(wps[0]);
            }
            else
                v = wps[0];

            for (int index = 0; index < length; ++index)
            {
                Vector3 wp = wps[index];
                this.Waypoints.AddLast(index >= length - 1
                    ? (index <= 0
                        ? this.CreateWaypoint(wp, Utility.Random(0.0f, 6.283185f))
                        : this.CreateWaypoint(wp, wp.GetAngleTowards(v)))
                    : this.CreateWaypoint(wp, wp.GetAngleTowards(wps[index + 1])));
            }
        }

        public LinkedListNode<WaypointEntry> AddWaypoint(Vector3 pos, float orientation)
        {
            return this.Waypoints.AddLast(this.CreateWaypoint(pos, orientation));
        }

        public LinkedListNode<WaypointEntry> InsertWaypointAfter(WaypointEntry entry, Vector3 pos, float orientation)
        {
            WaypointEntry waypoint = this.CreateWaypoint(pos, orientation);
            return this.Waypoints.AddAfter(entry.Node, waypoint);
        }

        public LinkedListNode<WaypointEntry> InsertWaypointBefore(WaypointEntry entry, Vector3 pos, float orientation)
        {
            WaypointEntry waypoint = this.CreateWaypoint(pos, orientation);
            return this.Waypoints.AddBefore(entry.Node, waypoint);
        }

        /// <summary>
        /// Increases the Id of all Waypoints, starting from the given node
        /// </summary>
        /// <param name="node">May be null</param>
        public void IncreaseWPIds(LinkedListNode<WaypointEntry> node)
        {
            for (; node != null; node = node.Next)
                ++node.Value.Id;
        }

        public override string ToString()
        {
            return string.Format(this.GetType().Name + " #{0} ({1} #{2})", (object) this.SpawnId, (object) this.EntryId,
                (object) this.EntryId);
        }

        public static IEnumerable<NPCSpawnEntry> GetAllDataHolders()
        {
            return (IEnumerable<NPCSpawnEntry>) NPCMgr.SpawnEntries;
        }
    }
}