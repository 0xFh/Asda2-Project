using System;
using System.Linq;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spawns
{
    [Serializable]
    public abstract class SpawnEntry<T, E, O, POINT, POOL> : ISpawnEntry, IWorldLocation, IHasPosition
        where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
        where E : SpawnEntry<T, E, O, POINT, POOL>
        where O : WorldObject
        where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
        where POOL : SpawnPool<T, E, O, POINT, POOL>
    {
        public readonly SynchronizedList<POINT> SpawnPoints = new SynchronizedList<POINT>(2);
        protected MapId m_MapId = MapId.End;
        private bool m_AutoSpawns = true;
        private static uint highestSpawnId;
        public uint SpawnId;
        public uint PoolId;
        public float PoolRespawnProbability;
        protected T m_PoolTemplate;
        public short _eventId;

        public static uint GenerateSpawnId()
        {
            return ++SpawnEntry<T, E, O, POINT, POOL>.highestSpawnId;
        }

        protected SpawnEntry()
        {
            this.Phase = 1U;
        }

        public MapId MapId
        {
            get { return this.m_MapId; }
            set { this.m_MapId = value; }
        }

        public Map Map
        {
            get { return WCell.RealmServer.Global.World.GetNonInstancedMap(this.MapId); }
        }

        /// <summary>The position of this SpawnEntry</summary>
        public Vector3 Position { get; set; }

        public float Orientation { get; set; }

        public uint Phase { get; set; }

        public int RespawnSeconds { get; set; }

        public int DespawnSeconds { get; set; }

        /// <summary>
        /// Min Delay in milliseconds until the unit should be respawned
        /// </summary>
        public int RespawnSecondsMin { get; set; }

        /// <summary>
        /// Max Delay in milliseconds until the unit should be respawned
        /// </summary>
        public int RespawnSecondsMax { get; set; }

        public uint EventId { get; set; }

        /// <summary>
        /// Whether this Entry spawns automatically (or is spawned by certain events)
        /// </summary>
        public bool AutoSpawns
        {
            get { return this.m_AutoSpawns; }
            set
            {
                if (this.m_AutoSpawns == value)
                    return;
                this.m_AutoSpawns = value;
                if ((object) this.m_PoolTemplate == null)
                    return;
                this.m_PoolTemplate.AutoSpawns =
                    this.m_PoolTemplate.Entries.Any<E>((Func<E, bool>) (entry => entry.AutoSpawns));
            }
        }

        [NotPersistent]
        public T PoolTemplate
        {
            get { return this.m_PoolTemplate; }
        }

        public int GetRandomRespawnMillis()
        {
            return 1000 * Utility.Random(this.RespawnSecondsMin, this.RespawnSecondsMax);
        }

        public abstract O SpawnObject(POINT point);

        public virtual void FinalizeDataHolder(bool addToPool)
        {
            if (this.RespawnSecondsMin == 0)
                this.RespawnSecondsMin = this.RespawnSeconds;
            if (this.RespawnSecondsMax == 0)
                this.RespawnSecondsMax = Math.Max(this.RespawnSeconds, this.RespawnSecondsMin);
            this.AutoSpawns = this.RespawnSecondsMax > 0;
            if (!this.AutoSpawns)
            {
                this.DespawnSeconds = -this.RespawnSeconds;
                this.RespawnSecondsMin = this.RespawnSecondsMax = 0;
            }

            if (this.Phase == 0U)
                this.Phase = 1U;
            if (this.SpawnId > SpawnEntry<T, E, O, POINT, POOL>.highestSpawnId)
            {
                SpawnEntry<T, E, O, POINT, POOL>.highestSpawnId = this.SpawnId;
            }
            else
            {
                if (this.SpawnId != 0U)
                    return;
                this.SpawnId = SpawnEntry<T, E, O, POINT, POOL>.GenerateSpawnId();
            }
        }
    }
}