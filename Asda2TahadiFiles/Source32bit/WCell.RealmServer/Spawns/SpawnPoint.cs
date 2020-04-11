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
    where E : SpawnEntry<T, E, O, POINT, POOL>
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
      m_timer = new TimerEntry(SpawnNow);
      Pool = pool;
      m_spawnEntry = entry;
    }

    public POOL Pool { get; protected set; }

    public E SpawnEntry
    {
      get { return m_spawnEntry; }
    }

    /// <summary>The currently active NPC of this SpawnPoint (or null)</summary>
    public O ActiveSpawnling
    {
      get { return m_spawnling; }
    }

    public Map Map
    {
      get { return Pool.Map; }
    }

    public MapId MapId
    {
      get { return Map.Id; }
    }

    public Vector3 Position
    {
      get { return m_spawnEntry.Position; }
    }

    public uint Phase
    {
      get { return m_spawnEntry.Phase; }
    }

    public bool HasSpawned
    {
      get { return m_spawnling != null; }
    }

    /// <summary>
    /// Whether timer is running and will spawn a new NPC when the timer elapses
    /// </summary>
    public bool IsSpawning
    {
      get { return m_timer.IsRunning; }
    }

    /// <summary>Pool active, but spawn inactive and npc autospawns</summary>
    public bool IsReadyToSpawn
    {
      get
      {
        if(Pool.IsActive && !IsActive && m_spawnEntry.AutoSpawns)
          return WorldEventMgr.IsEventActive(m_spawnEntry.EventId);
        return false;
      }
    }

    /// <summary>Whether NPC is already spawned or timer is running</summary>
    public bool IsActive
    {
      get
      {
        if(!HasSpawned)
          return IsSpawning;
        return true;
      }
    }

    public void Respawn()
    {
      Disable();
      SpawnNow();
    }

    private void SpawnNow(int dt)
    {
      SpawnNow();
    }

    public void SpawnNow()
    {
      Map.AddMessage(() =>
      {
        SpawnEntry.SpawnObject((POINT) this);
        Map.UnregisterUpdatable(m_timer);
      });
    }

    public void SpawnLater()
    {
      SpawnAfter(m_spawnEntry.GetRandomRespawnMillis());
    }

    /// <summary>Restarts the spawn timer with the given delay</summary>
    public void SpawnAfter(int delay)
    {
      if(!Pool.IsActive || m_timer.IsRunning)
        return;
      m_nextRespawn = Environment.TickCount + delay;
      m_timer.Start(delay);
      Map.RegisterUpdatableLater(m_timer);
    }

    /// <summary>Stops the Respawn timer, if it was running</summary>
    public void StopTimer()
    {
      m_timer.Stop();
      Map.UnregisterUpdatableLater(m_timer);
    }

    public void RemoveSpawnedObject()
    {
      O spawnling = m_spawnling;
      if(spawnling == null)
        return;
      spawnling.Delete();
    }

    /// <summary>Stops timer and deletes spawnling</summary>
    public void Disable()
    {
      StopTimer();
      RemoveSpawnedObject();
    }

    /// <summary>Called when object enters map</summary>
    protected internal void SignalSpawnlingActivated(O obj)
    {
      m_spawnling = obj;
      Pool.SpawnedObjects.Add(m_spawnling);
    }

    /// <summary>
    /// Is called when the given spawn died or was removed from Map.
    /// </summary>
    protected internal void SignalSpawnlingDied(O obj)
    {
      m_spawnling = default(O);
      Pool.SpawnedObjects.Remove(obj);
      if(!Pool.IsActive || !m_spawnEntry.AutoSpawns)
        return;
      Pool.SpawnOneLater();
    }
  }
}