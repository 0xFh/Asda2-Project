using System;
using System.Collections;
using System.Threading;
using WCell.Util.Collections;

namespace WCell.Util.ObjectPools
{
  /// <summary>This class represents a pool of objects.</summary>
  public class ObjectPool<T> : IObjectPool where T : class
  {
    /// <summary>A queue of objects in the pool.</summary>
    private readonly LockfreeQueue<object> _queue = new LockfreeQueue<object>();

    /// <summary>
    /// The minimum # of hard references that must be in the pool.
    /// </summary>
    private volatile int _minSize = 25;

    /// <summary>The number of hard references in the queue.</summary>
    private volatile int _hardReferences;

    private bool m_IsBalanced;

    /// <summary>The number of hard references in the queue.</summary>
    private volatile int _obtainedReferenceCount;

    /// <summary>Function pointer to the allocation function.</summary>
    private readonly Func<T> _createObj;

    /// <summary>
    /// Gets the number of hard references that are currently in the pool.
    /// </summary>
    public int HardReferenceCount
    {
      get { return _hardReferences; }
    }

    /// <summary>Gets the minimum size of the pool.</summary>
    public int MinimumSize
    {
      get { return _minSize; }
      set { _minSize = value; }
    }

    public int AvailableCount
    {
      get { return _queue.Count; }
    }

    public int ObtainedCount
    {
      get { return _obtainedReferenceCount; }
    }

    /// <summary>Gets information about the object pool.</summary>
    /// <value>A new <see cref="T:WCell.Util.ObjectPools.ObjectPoolInfo" /> object that contains information about the pool.</value>
    public ObjectPoolInfo Info
    {
      get
      {
        ObjectPoolInfo objectPoolInfo;
        objectPoolInfo.HardReferences = _hardReferences;
        objectPoolInfo.WeakReferences = _queue.Count - _hardReferences;
        return objectPoolInfo;
      }
    }

    public bool IsBalanced
    {
      get { return m_IsBalanced; }
      set { m_IsBalanced = value; }
    }

    /// <summary>Constructor</summary>
    /// <param name="func">Function pointer to the allocation function.</param>
    public ObjectPool(Func<T> func)
      : this(func, false)
    {
    }

    /// <summary>Constructor</summary>
    /// <param name="func">Function pointer to the allocation function.</param>
    public ObjectPool(Func<T> func, bool isBalanced)
    {
      IsBalanced = isBalanced;
      _createObj = func;
    }

    /// <summary>Adds an object to the queue.</summary>
    /// <param name="obj">The object to be added.</param>
    /// <remarks>If there are at least <see cref="P:WCell.Util.ObjectPools.ObjectPool`1.MinimumSize" /> hard references in the pool then the object is added as a WeakReference.
    /// A WeakReference allows an object to be collected by the GC if there are no other hard references to it.</remarks>
    public void Recycle(T obj)
    {
      if((object) obj is IPooledObject)
        ((IPooledObject) obj).Cleanup();
      if((object) obj is IList)
        ((IList) obj).Clear();
      if(_hardReferences >= _minSize)
      {
        _queue.Enqueue(new WeakReference(obj));
      }
      else
      {
        _queue.Enqueue(obj);
        Interlocked.Increment(ref _hardReferences);
      }

      if(!m_IsBalanced)
        return;
      OnRecycle();
    }

    /// <summary>Adds an object to the queue.</summary>
    /// <param name="obj">The object to be added.</param>
    /// <remarks>If there are at least <see cref="P:WCell.Util.ObjectPools.ObjectPool`1.MinimumSize" /> hard references in the pool then the object is added as a WeakReference.
    /// A WeakReference allows an object to be collected by the GC if there are no other hard references to it.</remarks>
    public void Recycle(object obj)
    {
      if(!(obj is T))
        return;
      if(obj is IPooledObject)
        ((IPooledObject) obj).Cleanup();
      if(_hardReferences >= _minSize)
      {
        _queue.Enqueue(new WeakReference(obj));
      }
      else
      {
        _queue.Enqueue(obj);
        Interlocked.Increment(ref _hardReferences);
      }

      if(m_IsBalanced)
        OnRecycle();
    }

    private void OnRecycle()
    {
      if(Interlocked.Decrement(ref _obtainedReferenceCount) < 0)
        throw new InvalidOperationException("Objects in Pool have been recycled too often: " + this);
    }

    /// <summary>Removes an object from the queue.</summary>
    /// <returns>An object from the queue or a new object if none were in the queue.</returns>
    public T Obtain()
    {
      if(m_IsBalanced)
        Interlocked.Increment(ref _obtainedReferenceCount);
      object obj;
      while(_queue.TryDequeue(out obj))
      {
        if(obj is WeakReference)
        {
          object target = ((WeakReference) obj).Target;
          if(target != null)
            return target as T;
        }
        else
        {
          Interlocked.Decrement(ref _hardReferences);
          return obj as T;
        }
      }

      return _createObj();
    }

    /// <summary>Removes an object from the queue.</summary>
    /// <returns>An object from the queue or a new object if none were in the queue.</returns>
    public object ObtainObj()
    {
      if(m_IsBalanced)
        Interlocked.Increment(ref _obtainedReferenceCount);
      object obj;
      while(_queue.TryDequeue(out obj))
      {
        WeakReference weakReference = obj as WeakReference;
        if(weakReference != null)
        {
          object target = weakReference.Target;
          if(target != null)
            return target;
        }
        else
        {
          Interlocked.Decrement(ref _hardReferences);
          return obj;
        }
      }

      return _createObj();
    }

    public override string ToString()
    {
      return GetType().Name + " for " + typeof(T).FullName;
    }

    public IDisposable Borrow(out T o)
    {
      o = Obtain();
      return new TempPoolGrant(this, o);
    }

    private struct TempPoolGrant : IDisposable
    {
      public ObjectPool<T> Pool;
      public T O;

      public TempPoolGrant(ObjectPool<T> pool, T o)
      {
        Pool = pool;
        O = o;
      }

      public void Dispose()
      {
        Pool.Recycle(O);
      }
    }
  }
}