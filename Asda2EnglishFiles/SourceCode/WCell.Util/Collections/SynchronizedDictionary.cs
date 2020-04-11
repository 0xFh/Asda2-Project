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
            get { return this._syncLock; }
        }

        public new virtual TValue this[TKey key]
        {
            get
            {
                Monitor.Enter(this._syncLock);
                try
                {
                    if (!base.ContainsKey(key))
                        throw new KeyNotFoundException();
                    return base[key];
                }
                finally
                {
                    Monitor.Exit(this._syncLock);
                }
            }
            set
            {
                Monitor.Enter(this._syncLock);
                try
                {
                    if (base.ContainsKey(key))
                        base[key] = value;
                    else
                        base.Add(key, value);
                }
                finally
                {
                    Monitor.Exit(this._syncLock);
                }
            }
        }

        public new virtual void Add(TKey key, TValue value)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                base.Add(key, value);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new virtual void Clear()
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

        public new bool ContainsKey(TKey key)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                return base.ContainsKey(key);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }
        }

        public new virtual bool Remove(TKey key)
        {
            Monitor.Enter(this._syncLock);
            try
            {
                if (!base.ContainsKey(key))
                    return false;
                base.Remove(key);
            }
            finally
            {
                Monitor.Exit(this._syncLock);
            }

            return true;
        }

        public void Lock()
        {
            Monitor.Enter(this._syncLock);
        }

        public void Unlock()
        {
            Monitor.Exit(this._syncLock);
        }
    }
}