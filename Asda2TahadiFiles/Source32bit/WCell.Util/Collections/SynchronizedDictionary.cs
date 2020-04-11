using System.Collections.Generic;
using System.Threading;

namespace WCell.Util.Collections
{
  public class SynchronizedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
  {
    private readonly object _syncLock = new object();

    public SynchronizedDictionary()
    {
    }

    public SynchronizedDictionary(IEqualityComparer<TKey> comparer)
      : base(comparer)
    {
    }

    public SynchronizedDictionary(int capacity)
      : base(capacity)
    {
    }

    public SynchronizedDictionary(IDictionary<TKey, TValue> dictionary)
      : base(dictionary)
    {
    }

    public object SyncLock
    {
      get { return _syncLock; }
    }

    public new virtual TValue this[TKey key]
    {
      get
      {
        Monitor.Enter(_syncLock);
        try
        {
          if(!base.ContainsKey(key))
            throw new KeyNotFoundException();
          return base[key];
        }
        finally
        {
          Monitor.Exit(_syncLock);
        }
      }
      set
      {
        Monitor.Enter(_syncLock);
        try
        {
          if(base.ContainsKey(key))
            base[key] = value;
          else
            base.Add(key, value);
        }
        finally
        {
          Monitor.Exit(_syncLock);
        }
      }
    }

    public new virtual void Add(TKey key, TValue value)
    {
      Monitor.Enter(_syncLock);
      try
      {
        base.Add(key, value);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new virtual void Clear()
    {
      Monitor.Enter(_syncLock);
      try
      {
        base.Clear();
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new bool ContainsKey(TKey key)
    {
      Monitor.Enter(_syncLock);
      try
      {
        return base.ContainsKey(key);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new virtual bool Remove(TKey key)
    {
      Monitor.Enter(_syncLock);
      try
      {
        if(!base.ContainsKey(key))
          return false;
        base.Remove(key);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }

      return true;
    }

    public void Lock()
    {
      Monitor.Enter(_syncLock);
    }

    public void Unlock()
    {
      Monitor.Exit(_syncLock);
    }
  }
}