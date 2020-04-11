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
                Monitor.Enter(this._syncLock);
                try
                {
                    return base.Count;
                }
                finally
                {
                    Monitor.Exit(this._syncLock);
                }
            }
        }

        public new void Clear()
        {
            Monitor.Enter(this._syncLock);
            try
            {
                base.Clear();
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new bool Contains(T obj)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                return base.Contains(obj);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                base.CopyTo(array, arrayIndex);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new T Dequeue()
        {
            Monitor.Enter(this._syncLock);
            try
            {
                return base.Dequeue();
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new void Enqueue(T value)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                base.Enqueue(value);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            Monitor.Enter(this._syncLock);
            try
            {
                return (IEnumerator<T>) base.GetEnumerator();
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new T Peek()
        {
            Monitor.Enter(this._syncLock);
            try
            {
                return base.Peek();
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new T[] ToArray()
        {
            Monitor.Enter(this._syncLock);
            try
            {
                return base.ToArray();
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new void TrimExcess()
        {
            Monitor.Enter(this._syncLock);
            try
            {
                base.TrimExcess();
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public void LockCollection()
        {
            Monitor.Enter(this._syncLock);
        }

        public void UnlockCollection()
        {
            Monitor.Exit(this._syncLock);
        }
    }
}