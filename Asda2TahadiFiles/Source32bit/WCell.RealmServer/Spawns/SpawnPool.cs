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
      Map = map;
      Template = templ;
      foreach(E entry in templ.Entries)
        AddSpawnPoint(entry);
    }

    public Map Map { get; protected set; }

    public T Template { get; protected set; }

    public abstract IList<O> SpawnedObjects { get; }

    public List<POINT> SpawnPoints
    {
      get { return m_spawnPoints; }
    }

    /// <summary>Whether all SpawnPoints of this pool are spawned</summary>
    public bool IsFullySpawned
    {
      get { return m_spawnPoints.Count(spawn => spawn.IsReadyToSpawn) == 0; }
    }

    /// <summary>
    /// Whether this Spawn point is actively spawning.
    /// Requires map context.
    /// </summary>
    public bool IsActive
    {
      get { return m_active; }
      set
      {
        if(m_active == value)
          return;
        m_active = value;
        if(m_active)
        {
          if(SpawnedObjects.Count == 0)
          {
            SpawnFull();
          }
          else
          {
            if(IsFullySpawned)
              return;
            SpawnOneLater();
          }
        }
        else
        {
          foreach(POINT spawnPoint in m_spawnPoints)
            spawnPoint.StopTimer();
        }
      }
    }

    public void Disable()
    {
      IsActive = false;
      RemoveAllSpawnedObjects();
    }

    public void RemoveAllSpawnedObjects()
    {
      foreach(POINT spawnPoint in m_spawnPoints)
        spawnPoint.RemoveSpawnedObject();
    }

    internal POINT AddSpawnPoint(E entry)
    {
      POINT instance = Activator.CreateInstance<POINT>();
      m_spawnPoints.Add(instance);
      instance.InitPoint((POOL) this, entry);
      return instance;
    }

    public POINT GetSpawnPoint(E entry)
    {
      return GetSpawnPoint(entry.SpawnId);
    }

    public POINT GetSpawnPoint(uint spawnId)
    {
      Map.EnsureContext();
      return SpawnPoints.FirstOrDefault(point =>
        (int) point.SpawnEntry.SpawnId == (int) spawnId);
    }

    /// <summary>Returns a spawn point that is currently inactive</summary>
    public POINT GetRandomInactiveSpawnPoint()
    {
      Map.EnsureContext();
      float num1 = 0.0f;
      int num2 = 0;
      int num3 = 0;
      foreach(POINT spawnPoint in m_spawnPoints)
      {
        if(spawnPoint.IsReadyToSpawn)
        {
          float respawnProbability = spawnPoint.SpawnEntry.PoolRespawnProbability;
          num1 += respawnProbability;
          ++num2;
          if(respawnProbability == 0.0)
            ++num3;
        }
      }

      if(num2 == 0)
        return default(POINT);
      float num4 = 100f / num2;
      float num5 = Utility.RandomFloat() * (num1 + num3 * num4);
      float num6 = 0.0f;
      foreach(POINT spawnPoint in m_spawnPoints)
      {
        if(spawnPoint.IsReadyToSpawn)
        {
          float num7 = spawnPoint.SpawnEntry.PoolRespawnProbability;
          if(num7 == 0.0)
            num7 = num4;
          num6 += num7;
          if(num5 <= (double) num6)
            return spawnPoint;
        }
      }

      return default(POINT);
    }

    /// <summary>Spawns NPCs until MaxAmount of NPCs are spawned.</summary>
    public void SpawnFull()
    {
      Map.EnsureContext();
      POINT inactiveSpawnPoint;
      for(int count = SpawnedObjects.Count;
        count < Template.RealMaxSpawnAmount &&
        (object) (inactiveSpawnPoint = GetRandomInactiveSpawnPoint()) != null;
        ++count)
        inactiveSpawnPoint.SpawnNow();
    }

    public void RespawnFull()
    {
      Clear();
      SpawnFull();
    }

    /// <summary>
    /// Spawns an NPC from a random inactive spawn point or returns false, if all SpawnPoints are active
    /// </summary>
    public bool SpawnOneNow()
    {
      POINT inactiveSpawnPoint = GetRandomInactiveSpawnPoint();
      if(inactiveSpawnPoint == null)
        return false;
      inactiveSpawnPoint.SpawnNow();
      return true;
    }

    public bool SpawnOneLater()
    {
      POINT inactiveSpawnPoint = GetRandomInactiveSpawnPoint();
      if(inactiveSpawnPoint == null)
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
      Map.EnsureContext();
      foreach(O o in SpawnedObjects.ToArray())
        o.DeleteNow();
    }

    public void RemovePoolLater()
    {
      Map.AddMessage(() => RemovePoolNow());
    }

    public void RemovePoolNow()
    {
      if(Map == null)
        return;
      IsActive = false;
      Clear();
      Map.RemoveSpawnPool<T, E, O, POINT, POOL>((POOL) this);
      Map = null;
    }
  }
}