using System.Collections.Generic;
using System.Threading;

namespace WCell.Util.Collections
{
  public class SynchronizedQueue<T> : Queue<T>
  {
    private readonly object _syncLock = new object();

    public SynchronizedQueue()
    {
    }

    public SynchronizedQueue(int capacity)
      : base(capacity)
    {
    }

    public SynchronizedQueue(IEnumerable<T> collection)
      : base(collection)
    {
    }

    public new int Count
    {
      get
      {
        Monitor.Enter(_syncLock);
        try
        {
          return base.Count;
        }
        finally
        {
          Monitor.Exit(_syncLock);
        }
      }
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

    public new bool Contains(T obj)
    {
      Monitor.Enter(_syncLock);
      try
      {
        return base.Contains(obj);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new void CopyTo(T[] array, int arrayIndex)
    {
      Monitor.Enter(_syncLock);
      try
      {
        base.CopyTo(array, arrayIndex);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new T Dequeue()
    {
      Monitor.Enter(_syncLock);
      try
      {
        return base.Dequeue();
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new void Enqueue(T value)
    {
      Monitor.Enter(_syncLock);
      try
      {
        base.Enqueue(value);
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      Monitor.Enter(_syncLock);
      try
      {
        return base.GetEnumerator();
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new T Peek()
    {
      Monitor.Enter(_syncLock);
      try
      {
        return base.Peek();
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new T[] ToArray()
    {
      Monitor.Enter(_syncLock);
      try
      {
        return base.ToArray();
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public new void TrimExcess()
    {
      Monitor.Enter(_syncLock);
      try
      {
        base.TrimExcess();
      }
      finally
      {
        Monitor.Exit(_syncLock);
      }
    }

    public void LockCollection()
    {
      Monitor.Enter(_syncLock);
    }

    public void UnlockCollection()
    {
      Monitor.Exit(_syncLock);
    }
  }
}