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
                if (index > this.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                Monitor.Enter(this._syncLock);
                T obj;
                try
                {
                    obj = base[index];
                }
                finally
                {
                    Monitor.Exit(this._syncLock);
                }

                return obj;
            }
            set
            {
                if (index > this.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                Monitor.Enter(this._syncLock);
                try
                {
                    base[index] = value;
                }
                finally
                {
                    Monitor.Exit(this._syncLock);
                }
            }
        }

        public new void Add(T value)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                base.Add(value);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new bool Remove(T value)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                return base.Remove(value);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new void RemoveAt(int index)
        {
            if (index > this.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            Monitor.Enter(this._syncLock);
            try
            {
                base.RemoveAt(index);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        protected void RemoveUnlocked(int index)
        {
            base.RemoveAt(index);
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

        public new bool Contains(T item)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                return base.Contains(item);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public void EnterLock()
        {
            Monitor.Enter(this._syncLock);
        }

        public void ExitLock()
        {
            Monitor.Exit(this._syncLock);
        }
    }
}