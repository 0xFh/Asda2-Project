using System;
using System.Collections.Generic;
using System.Threading;

namespace WCell.Util.Collections
{
  [Serializable]
  public class SynchronizedList<T> : List<T>
  {
    private readonly object _syncLock = new object();

    public SynchronizedList()
    {
    }

    public SynchronizedList(int capacity)
      : base(capacity)
    {
    }

    public SynchronizedList(IEnumerable<T> collection)
      : base(collection)
    {
    }

    public new T this[int index]
    {
      get
      {
        if(index > Count)
          throw new ArgumentOutOfRangeException(nameof(index));
        Monitor.Enter(_syncLock);
        T obj;
        try
        {
          obj = base[index];
        }
        finally
        {
          Monitor.Exit(_syncLock);
        }

        return obj;
      }
      set
      {
        if(index > Count)
          throw new ArgumentOutOfRangeException(nameof(index));
        Monitor.Enter(_syncLock);
        try
        {
          base[index] = value;
        }
        finally
        {
          Monitor.Exit(_syncLock);
        }
      }
    }

    public new void Add(T value)
    {
      Monitor.Enter(_syncLock);
      try
      {
        base.Add(value);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new bool Remove(T value)
    {
      Monitor.Enter(_syncLock);
      try
      {
        return base.Remove(value);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new void RemoveAt(int index)
    {
      if(index > Count)
        throw new ArgumentOutOfRangeException(nameof(index));
      Monitor.Enter(_syncLock);
      try
      {
        base.RemoveAt(index);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    protected void RemoveUnlocked(int index)
    {
      base.RemoveAt(index);
    }

    public new void Clear()
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

    public new bool Contains(T item)
    {
      Monitor.Enter(_syncLock);
      try
      {
        return base.Contains(item);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public void EnterLock()
    {
      Monitor.Enter(_syncLock);
    }

    public void ExitLock()
    {
      Monitor.Exit(_syncLock);
    }
  }
}