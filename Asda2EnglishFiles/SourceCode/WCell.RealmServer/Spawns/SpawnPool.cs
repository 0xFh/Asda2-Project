using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Spawns
{
    public abstract class SpawnPool<T, E, O, POINT, POOL> where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
        where E : SpawnEntry<T, E, O, POINT, POOL>
        where O : WorldObject
        where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
        where POOL : SpawnPool<T, E, O, POINT, POOL>
    {
        protected internal List<POINT> m_spawnPoints = new List<POINT>(5);
        protected bool m_active;

        protected SpawnPool(Map map, T templ)
        {
            this.Map = map;
            this.Template = templ;
            foreach (E entry in templ.Entries)
                this.AddSpawnPoint(entry);
        }

        public Map Map { get; protected set; }

        public T Template { get; protected set; }

        public abstract IList<O> SpawnedObjects { get; }

        public List<POINT> SpawnPoints
        {
            get { return this.m_spawnPoints; }
        }

        /// <summary>Whether all SpawnPoints of this pool are spawned</summary>
        public bool IsFullySpawned
        {
            get { return this.m_spawnPoints.Count<POINT>((Func<POINT, bool>) (spawn => spawn.IsReadyToSpawn)) == 0; }
        }

        /// <summary>
        /// Whether this Spawn point is actively spawning.
        /// Requires map context.
        /// </summary>
        public bool IsActive
        {
            get { return this.m_active; }
            set
            {
                if (this.m_active == value)
                    return;
                this.m_active = value;
                if (this.m_active)
                {
                    if (this.SpawnedObjects.Count == 0)
                    {
                        this.SpawnFull();
                    }
                    else
                    {
                        if (this.IsFullySpawned)
                            return;
                        this.SpawnOneLater();
                    }
                }
                else
                {
                    foreach (POINT spawnPoint in this.m_spawnPoints)
                        spawnPoint.StopTimer();
                }
            }
        }

        public void Disable()
        {
            this.IsActive = false;
            this.RemoveAllSpawnedObjects();
        }

        public void RemoveAllSpawnedObjects()
        {
            foreach (POINT spawnPoint in this.m_spawnPoints)
                spawnPoint.RemoveSpawnedObject();
        }

        internal POINT AddSpawnPoint(E entry)
        {
            POINT instance = Activator.CreateInstance<POINT>();
            this.m_spawnPoints.Add(instance);
            instance.InitPoint((POOL) this, entry);
            return instance;
        }

        public POINT GetSpawnPoint(E entry)
        {
            return this.GetSpawnPoint(entry.SpawnId);
        }

        public POINT GetSpawnPoint(uint spawnId)
        {
            this.Map.EnsureContext();
            return this.SpawnPoints.FirstOrDefault<POINT>((Func<POINT, bool>) (point =>
                (int) point.SpawnEntry.SpawnId == (int) spawnId));
        }

        /// <summary>Returns a spawn point that is currently inactive</summary>
        public POINT GetRandomInactiveSpawnPoint()
        {
            this.Map.EnsureContext();
            float num1 = 0.0f;
            int num2 = 0;
            int num3 = 0;
            foreach (POINT spawnPoint in this.m_spawnPoints)
            {
                if (spawnPoint.IsReadyToSpawn)
                {
                    float respawnProbability = spawnPoint.SpawnEntry.PoolRespawnProbability;
                    num1 += respawnProbability;
                    ++num2;
                    if ((double) respawnProbability == 0.0)
                        ++num3;
                }
            }

            if (num2 == 0)
                return default(POINT);
            float num4 = 100f / (float) num2;
            float num5 = Utility.RandomFloat() * (num1 + (float) num3 * num4);
            float num6 = 0.0f;
            foreach (POINT spawnPoint in this.m_spawnPoints)
            {
                if (spawnPoint.IsReadyToSpawn)
                {
                    float num7 = spawnPoint.SpawnEntry.PoolRespawnProbability;
                    if ((double) num7 == 0.0)
                        num7 = num4;
                    num6 += num7;
                    if ((double) num5 <= (double) num6)
                        return spawnPoint;
                }
            }

            return default(POINT);
        }

        /// <summary>Spawns NPCs until MaxAmount of NPCs are spawned.</summary>
        public void SpawnFull()
        {
            this.Map.EnsureContext();
            POINT inactiveSpawnPoint;
            for (int count = this.SpawnedObjects.Count;
                count < this.Template.RealMaxSpawnAmount &&
                (object) (inactiveSpawnPoint = this.GetRandomInactiveSpawnPoint()) != null;
                ++count)
                inactiveSpawnPoint.SpawnNow();
        }

        public void RespawnFull()
        {
            this.Clear();
            this.SpawnFull();
        }

        /// <summary>
        /// Spawns an NPC from a random inactive spawn point or returns false, if all SpawnPoints are active
        /// </summary>
        public bool SpawnOneNow()
        {
            POINT inactiveSpawnPoint = this.GetRandomInactiveSpawnPoint();
            if ((object) inactiveSpawnPoint == null)
                return false;
            inactiveSpawnPoint.SpawnNow();
            return true;
        }

        public bool SpawnOneLater()
        {
            POINT inactiveSpawnPoint = this.GetRandomInactiveSpawnPoint();
            if ((object) inactiveSpawnPoint == null)
                return false;
            inactiveSpawnPoint.SpawnLater();
            return true;
        }

        /// <summary>
        /// Removes all spawned NPCs from this pool and makes it
        /// collectable by the GC (if not in Map anymore).
        /// </summary>
        /// <remarks>Requires map context</remarks>
        public void Clear()
        {
            this.Map.EnsureContext();
            foreach (O o in this.SpawnedObjects.ToArray<O>())
                o.DeleteNow();
        }

        public void RemovePoolLater()
        {
            this.Map.AddMessage((Action) (() => this.RemovePoolNow()));
        }

        public void RemovePoolNow()
        {
            if (this.Map == null)
                return;
            this.IsActive = false;
            this.Clear();
            this.Map.RemoveSpawnPool<T, E, O, POINT, POOL>((POOL) this);
            this.Map = (Map) null;
        }
    }
}