using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Spawns;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects.Spawns
{
    /// <summary>Spawn-information for GameObjects</summary>
    [DataHolder]
    public class GOSpawnEntry : SpawnEntry<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>,
        IDataHolder
    {
        public float Scale = 1f;
        public uint Id;
        public GOEntryId EntryId;
        [NotPersistent] public uint EntryIdRaw;
        public GameObjectState State;
        [Persistent(4)] public float[] Rotations;
        public byte AnimProgress;
        [NotPersistent] public GOEntry Entry;
        [NotPersistent] public LootItemEntry LootEntry;

        public GOSpawnEntry()
        {
        }

        public GOSpawnEntry(GOEntry entry, GameObjectState state, MapId mapId, ref Vector3 pos, float orientation,
            float scale, float[] rotations, int respawnTimeSecs = 600)
        {
            this.Entry = entry;
            this.EntryId = entry.GOId;
            this.State = state;
            this.MapId = mapId;
            this.Position = pos;
            this.Orientation = orientation;
            this.Scale = scale;
            this.Rotations = rotations;
            this.RespawnSeconds = respawnTimeSecs;
        }

        public uint LootMoney
        {
            get { return 0; }
        }

        /// <summary>
        /// Spawns and returns a new GameObject from this template into the given map
        /// </summary>
        /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
        public GameObject Spawn(Map map)
        {
            if (this.Entry == null)
                return (GameObject) null;
            return GameObject.Create(this.Entry, (IWorldLocation) new WorldLocationStruct(map, this.Position, 1U), this,
                (GOSpawnPoint) null);
        }

        /// <summary>
        /// Spawns and returns a new GameObject from this template at the given location
        /// </summary>
        /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
        public GameObject Spawn(IWorldLocation pos)
        {
            return GameObject.Create(this.Entry, pos, this, (GOSpawnPoint) null);
        }

        public override GameObject SpawnObject(GOSpawnPoint point)
        {
            return GameObject.Create(this.Entry, (IWorldLocation) point, this, point);
        }

        public uint GetId()
        {
            return this.Id;
        }

        public DataHolderState DataHolderState { get; set; }

        /// <summary>Finalize this GOSpawnEntry</summary>
        /// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom GOSpawnEntry that share a pool)</param>
        public void FinalizeDataHolder()
        {
            this.FinalizeDataHolder(true);
        }

        private void AddToPoolTemplate()
        {
            this.m_PoolTemplate = GOMgr.GetOrCreateSpawnPoolTemplate(this.PoolId);
            this.m_PoolTemplate.AddEntry(this);
        }

        /// <summary>Finalize this GOSpawnEntry</summary>
        /// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom GOSpawnEntry that share a pool)</param>
        public override void FinalizeDataHolder(bool addToPool)
        {
            if (this.Entry == null)
            {
                this.Entry = GOMgr.GetEntry(this.EntryId, false);
                if (this.Entry == null)
                {
                    ContentMgr.OnInvalidDBData("{0} had an invalid EntryId.", (object) this);
                    return;
                }
            }

            if ((double) this.Scale == 0.0)
                this.Scale = 1f;
            if (this.EntryId == GOEntryId.Loot)
                this.EntryId = (GOEntryId) this.EntryIdRaw;
            else
                this.EntryIdRaw = (uint) this.EntryId;
            if (this.Rotations == null)
                this.Rotations = new float[4];
            base.FinalizeDataHolder(addToPool);
            if (this.MapId == MapId.End || this.MapId == MapId.None)
                return;
            this.Entry.SpawnEntries.Add(this);
            ArrayUtil.Set<GOSpawnEntry>(ref GOMgr.SpawnEntries, this.SpawnId, this);
            if (!addToPool)
                return;
            this.AddToPoolTemplate();
        }

        public override string ToString()
        {
            return (this.Entry != null ? (object) this.Entry.DefaultName : (object) "").ToString() + " (EntryId: " +
                   (object) this.EntryId + " (" + (object) (int) this.EntryId + "))";
        }

        /// <summary>Do not remove: Used internally for caching</summary>
        public static IEnumerable<GOSpawnEntry> GetAllDataHolders()
        {
            List<GOSpawnEntry> goSpawnEntryList = new List<GOSpawnEntry>(10000);
            foreach (GOEntry goEntry in GOMgr.Entries.Values)
            {
                if (goEntry != null)
                    goSpawnEntryList.AddRange((IEnumerable<GOSpawnEntry>) goEntry.SpawnEntries);
            }

            return (IEnumerable<GOSpawnEntry>) goSpawnEntryList;
        }
    }
}