using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Util;

namespace WCell.RealmServer.Global
{
  public class WorldInstanceCollection<TE, TM> where TE : struct, IConvertible where TM : InstancedMap
  {
    private readonly ReaderWriterLockSlim _lck = new ReaderWriterLockSlim();
    internal TM[][] Instances;
    private int _count;

    public WorldInstanceCollection(TE size)
    {
      Instances = new TM[size.ToInt32(null)][];
    }

    public int Count
    {
      get { return _count; }
    }

    /// <summary>Gets an instance</summary>
    /// <returns>the <see cref="T:WCell.RealmServer.Global.Map" /> object; null if the ID is not valid</returns>
    /// s
    public TM GetInstance(TE mapId, uint instanceId)
    {
      TM[] arr = Instances.Get(mapId.ToUInt32(null));
      if(arr == null)
        return default(TM);
      return arr.Get(instanceId);
    }

    public TM[] GetInstances(TE map)
    {
      TM[] mArray = Instances.Get(map.ToUInt32(null));
      if(mArray == null)
        return new TM[0];
      return mArray.Where(instance => (object) instance != null)
        .ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    /// <remarks>Never returns null</remarks>
    public TM[] GetOrCreateInstances(TE map)
    {
      TM[] mArray = Instances.Get(map.ToUInt32(null));
      if(mArray != null)
        return mArray;
      _lck.EnterWriteLock();
      try
      {
        Instances[map.ToUInt32(null)] = mArray = new TM[10];
      }
      finally
      {
        _lck.ExitWriteLock();
      }

      return mArray;
    }

    public List<TM> GetAllInstances()
    {
      List<TM> mList = new List<TM>();
      _lck.EnterReadLock();
      try
      {
        foreach(TM[] instance1 in Instances)
        {
          if(instance1 != null)
            mList.AddRange(
              instance1.Where(instance =>
                (object) instance != null));
        }
      }
      finally
      {
        _lck.ExitReadLock();
      }

      return mList;
    }

    internal void AddInstance(TE id, TM map)
    {
      TM[] instances = GetOrCreateInstances(id);
      if(map.InstanceId >= instances.Length)
      {
        _lck.EnterWriteLock();
        try
        {
          instances = GetOrCreateInstances(id);
          Array.Resize(ref instances, (int) (map.InstanceId * 1.5));
          Instances[id.ToUInt32(null)] = instances;
        }
        finally
        {
          _lck.ExitWriteLock();
        }
      }

      instances[map.InstanceId] = map;
      Interlocked.Increment(ref _count);
    }

    internal void RemoveInstance(TE mapId, uint instanceId)
    {
      _lck.EnterWriteLock();
      try
      {
        GetOrCreateInstances(mapId)[instanceId] = default(TM);
        --_count;
      }
      finally
      {
        _lck.ExitWriteLock();
      }
    }
  }
}