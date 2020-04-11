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
        private volatile int _hardReferences = 0;

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
            get { return this._hardReferences; }
        }

        /// <summary>Gets the minimum size of the pool.</summary>
        public int MinimumSize
        {
            get { return this._minSize; }
            set { this._minSize = value; }
        }

        public int AvailableCount
        {
            get { return this._queue.Count; }
        }

        public int ObtainedCount
        {
            get { return this._obtainedReferenceCount; }
        }

        /// <summary>Gets information about the object pool.</summary>
        /// <value>A new <see cref="T:WCell.Util.ObjectPools.ObjectPoolInfo" /> object that contains information about the pool.</value>
        public ObjectPoolInfo Info
        {
            get
            {
                ObjectPoolInfo objectPoolInfo;
                objectPoolInfo.HardReferences = this._hardReferences;
                objectPoolInfo.WeakReferences = this._queue.Count - this._hardReferences;
                return objectPoolInfo;
            }
        }

        public bool IsBalanced
        {
            get { return this.m_IsBalanced; }
            set { this.m_IsBalanced = value; }
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
            this.IsBalanced = isBalanced;
            this._createObj = func;
        }

        /// <summary>Adds an object to the queue.</summary>
        /// <param name="obj">The object to be added.</param>
        /// <remarks>If there are at least <see cref="P:WCell.Util.ObjectPools.ObjectPool`1.MinimumSize" /> hard references in the pool then the object is added as a WeakReference.
        /// A WeakReference allows an object to be collected by the GC if there are no other hard references to it.</remarks>
        public void Recycle(T obj)
        {
            if ((object) obj is IPooledObject)
                ((IPooledObject) (object) obj).Cleanup();
            if ((object) obj is IList)
                ((IList) (object) obj).Clear();
            if (this._hardReferences >= this._minSize)
            {
                this._queue.Enqueue((object) new WeakReference((object) obj));
            }
            else
            {
                this._queue.Enqueue((object) obj);
                Interlocked.Increment(ref this._hardReferences);
            }

            if (!this.m_IsBalanced)
                return;
            this.OnRecycle();
        }

        /// <summary>Adds an object to the queue.</summary>
        /// <param name="obj">The object to be added.</param>
        /// <remarks>If there are at least <see cref="P:WCell.Util.ObjectPools.ObjectPool`1.MinimumSize" /> hard references in the pool then the object is added as a WeakReference.
        /// A WeakReference allows an object to be collected by the GC if there are no other hard references to it.</remarks>
        public void Recycle(object obj)
        {
            if (!(obj is T))
                return;
            if (obj is IPooledObject)
                ((IPooledObject) obj).Cleanup();
            if (this._hardReferences >= this._minSize)
            {
                this._queue.Enqueue((object) new WeakReference(obj));
            }
            else
            {
                this._queue.Enqueue(obj);
                Interlocked.Increment(ref this._hardReferences);
            }

            if (this.m_IsBalanced)
                this.OnRecycle();
        }

        private void OnRecycle()
        {
            if (Interlocked.Decrement(ref this._obtainedReferenceCount) < 0)
                throw new InvalidOperationException("Objects in Pool have been recycled too often: " + (object) this);
        }

        /// <summary>Removes an object from the queue.</summary>
        /// <returns>An object from the queue or a new object if none were in the queue.</returns>
        public T Obtain()
        {
            if (this.m_IsBalanced)
                Interlocked.Increment(ref this._obtainedReferenceCount);
            object obj;
            while (this._queue.TryDequeue(out obj))
            {
                if (obj is WeakReference)
                {
                    object target = ((WeakReference) obj).Target;
                    if (target != null)
                        return target as T;
                }
                else
                {
                    Interlocked.Decrement(ref this._hardReferences);
                    return obj as T;
                }
            }

            return this._createObj();
        }

        /// <summary>Removes an object from the queue.</summary>
        /// <returns>An object from the queue or a new object if none were in the queue.</returns>
        public object ObtainObj()
        {
            if (this.m_IsBalanced)
                Interlocked.Increment(ref this._obtainedReferenceCount);
            object obj;
            while (this._queue.TryDequeue(out obj))
            {
                WeakReference weakReference = obj as WeakReference;
                if (weakReference != null)
                {
                    object target = weakReference.Target;
                    if (target != null)
                        return target;
                }
                else
                {
                    Interlocked.Decrement(ref this._hardReferences);
                    return obj;
                }
            }

            return (object) this._createObj();
        }

        public override string ToString()
        {
            return this.GetType().Name + " for " + typeof(T).FullName;
        }

        public IDisposable Borrow(out T o)
        {
            o = this.Obtain();
            return (IDisposable) new ObjectPool<T>.TempPoolGrant(this, o);
        }

        private struct TempPoolGrant : IDisposable
        {
            public ObjectPool<T> Pool;
            public T O;

            public TempPoolGrant(ObjectPool<T> pool, T o)
            {
                this.Pool = pool;
                this.O = o;
            }

            public void Dispose()
            {
                this.Pool.Recycle(this.O);
            }
        }
    }
}