using System;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spawns
{
    [Serializable]
    public abstract class SpawnPoint<T, E, O, POINT, POOL> : ISpawnPoint
        where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
        where E : WCell.RealmServer.Spawns.SpawnEntry<T, E, O, POINT, POOL>
        where O : WorldObject
        where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
        where POOL : SpawnPool<T, E, O, POINT, POOL>
    {
        protected E m_spawnEntry;
        protected internal TimerEntry m_timer;
        protected int m_nextRespawn;
        protected O m_spawnling;

        internal void InitPoint(POOL pool, E entry)
        {
            this.m_timer = new TimerEntry(new Action<int>(this.SpawnNow));
            this.Pool = pool;
            this.m_spawnEntry = entry;
        }

        public POOL Pool { get; protected set; }

        public E SpawnEntry
        {
            get { return this.m_spawnEntry; }
        }

        /// <summary>The currently active NPC of this SpawnPoint (or null)</summary>
        public O ActiveSpawnling
        {
            get { return this.m_spawnling; }
        }

        public Map Map
        {
            get { return this.Pool.Map; }
        }

        public MapId MapId
        {
            get { return this.Map.Id; }
        }

        public Vector3 Position
        {
            get { return this.m_spawnEntry.Position; }
        }

        public uint Phase
        {
            get { return this.m_spawnEntry.Phase; }
        }

        public bool HasSpawned
        {
            get { return (object) this.m_spawnling != null; }
        }

        /// <summary>
        /// Whether timer is running and will spawn a new NPC when the timer elapses
        /// </summary>
        public bool IsSpawning
        {
            get { return this.m_timer.IsRunning; }
        }

        /// <summary>Pool active, but spawn inactive and npc autospawns</summary>
        public bool IsReadyToSpawn
        {
            get
            {
                if (this.Pool.IsActive && !this.IsActive && this.m_spawnEntry.AutoSpawns)
                    return WorldEventMgr.IsEventActive(this.m_spawnEntry.EventId);
                return false;
            }
        }

        /// <summary>Whether NPC is already spawned or timer is running</summary>
        public bool IsActive
        {
            get
            {
                if (!this.HasSpawned)
                    return this.IsSpawning;
                return true;
            }
        }

        public void Respawn()
        {
            this.Disable();
            this.SpawnNow();
        }

        private void SpawnNow(int dt)
        {
            this.SpawnNow();
        }

        public void SpawnNow()
        {
            this.Map.AddMessage((Action) (() =>
            {
                this.SpawnEntry.SpawnObject((POINT) this);
                this.Map.UnregisterUpdatable((IUpdatable) this.m_timer);
            }));
        }

        public void SpawnLater()
        {
            this.SpawnAfter(this.m_spawnEntry.GetRandomRespawnMillis());
        }

        /// <summary>Restarts the spawn timer with the given delay</summary>
        public void SpawnAfter(int delay)
        {
            if (!this.Pool.IsActive || this.m_timer.IsRunning)
                return;
            this.m_nextRespawn = Environment.TickCount + delay;
            this.m_timer.Start(delay);
            this.Map.RegisterUpdatableLater((IUpdatable) this.m_timer);
        }

        /// <summary>Stops the Respawn timer, if it was running</summary>
        public void StopTimer()
        {
            this.m_timer.Stop();
            this.Map.UnregisterUpdatableLater((IUpdatable) this.m_timer);
        }

        public void RemoveSpawnedObject()
        {
            O spawnling = this.m_spawnling;
            if ((object) spawnling == null)
                return;
            spawnling.Delete();
        }

        /// <summary>Stops timer and deletes spawnling</summary>
        public void Disable()
        {
            this.StopTimer();
            this.RemoveSpawnedObject();
        }

        /// <summary>Called when object enters map</summary>
        protected internal void SignalSpawnlingActivated(O obj)
        {
            this.m_spawnling = obj;
            this.Pool.SpawnedObjects.Add(this.m_spawnling);
        }

        /// <summary>
        /// Is called when the given spawn died or was removed from Map.
        /// </summary>
        protected internal void SignalSpawnlingDied(O obj)
        {
            this.m_spawnling = default(O);
            this.Pool.SpawnedObjects.Remove(obj);
            if (!this.Pool.IsActive || !this.m_spawnEntry.AutoSpawns)
                return;
            this.Pool.SpawnOneLater();
        }
    }
}