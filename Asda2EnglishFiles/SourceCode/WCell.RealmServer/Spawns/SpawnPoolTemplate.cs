using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.Util.Data;

namespace WCell.RealmServer.Spawns
{
    [Serializable]
    public abstract class SpawnPoolTemplate<T, E, O, POINT, POOL> where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
        where E : SpawnEntry<T, E, O, POINT, POOL>
        where O : WorldObject
        where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
        where POOL : SpawnPool<T, E, O, POINT, POOL>
    {
        private static int highestId = 1000000;
        [NotPersistent] public MapId MapId = MapId.End;
        [NotPersistent] public List<E> Entries = new List<E>(5);
        public uint PoolId;
        public int MaxSpawnAmount;

        public int RealMaxSpawnAmount
        {
            get
            {
                return Math.Min(this.MaxSpawnAmount,
                    this.Entries.Count<E>((Func<E, bool>) (entry => entry.AutoSpawns)));
            }
        }

        /// <summary>Whether any SpawnEntry has AutoSpawns set to true</summary>
        [NotPersistent]
        public bool AutoSpawns { get; internal set; }

        /// <summary>
        /// It would not make sense to create a pool that contains entries of different events
        /// </summary>
        public uint EventId
        {
            get { return this.Entries[0].EventId; }
        }

        protected SpawnPoolTemplate(uint id, int maxSpawnAmount)
        {
            this.AutoSpawns = false;
            this.PoolId = id != 0U
                ? id
                : (uint) Interlocked.Increment(ref SpawnPoolTemplate<T, E, O, POINT, POOL>.highestId);
            this.MaxSpawnAmount = maxSpawnAmount != 0 ? maxSpawnAmount : int.MaxValue;
        }

        protected SpawnPoolTemplate(SpawnPoolTemplateEntry entry)
            : this(entry.PoolId, entry.MaxSpawnAmount)
        {
        }

        public abstract List<T> PoolTemplatesOnSameMap { get; }

        public void AddEntry(E entry)
        {
            if (this.MapId == MapId.End)
            {
                this.MapId = entry.MapId;
                if (entry.MapId != MapId.End)
                    this.PoolTemplatesOnSameMap.Add((T) this);
            }
            else if (entry.MapId != this.MapId)
            {
                LogManager.GetCurrentClassLogger()
                    .Warn("Tried to add \"{0}\" with map = \"{1}\" to a pool that contains Entries of Map \"{2}\"",
                        (object) entry, (object) entry.MapId, (object) this.MapId);
                return;
            }

            this.Entries.Add(entry);
            this.AutoSpawns = this.AutoSpawns || entry.AutoSpawns;
        }
    }
}