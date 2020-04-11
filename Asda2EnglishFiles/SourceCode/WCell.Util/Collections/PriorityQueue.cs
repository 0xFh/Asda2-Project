using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace WCell.Util.Collections
{
    [Serializable]
    public class PriorityQueue<T> : IEnumerable<T>, IEnumerable, ISerializable
    {
        private const string CAPACITY_NAME = "capacity";
        private const string COUNT_NAME = "count";
        private const string HEAP_NAME = "heap";
        private int _count;
        private int _capacity;
        private int _version;
        private HeapEntry<T>[] _heap;

        public PriorityQueue()
        {
            this._capacity = 15;
            this._heap = new HeapEntry<T>[this._capacity];
        }

        protected PriorityQueue(SerializationInfo info, StreamingContext context)
        {
            this._capacity = info.GetInt32("capacity");
            this._count = info.GetInt32("count");
            HeapEntry<T>[] heapEntryArray = (HeapEntry<T>[]) info.GetValue("heap", typeof(HeapEntry<T>[]));
            this._heap = new HeapEntry<T>[this._capacity];
            Array.Copy((Array) heapEntryArray, 0, (Array) this._heap, 0, this._count);
            this._version = 0;
        }

        public T Dequeue()
        {
            if (this._count == 0)
                throw new InvalidOperationException();
            T obj = this._heap[0].Item;
            --this._count;
            this.TrickleDown(0, this._heap[this._count]);
            this._heap[this._count].Clear();
            ++this._version;
            return obj;
        }

        public void Enqueue(T item, IComparable priority)
        {
            if (priority == null)
                throw new ArgumentNullException(nameof(priority));
            if (this._count == this._capacity)
                this.GrowHeap();
            ++this._count;
            this.BubbleUp(this._count - 1, new HeapEntry<T>(item, priority));
            ++this._version;
        }

        private void BubbleUp(int index, HeapEntry<T> he)
        {
            for (int parent = this.GetParent(index);
                index > 0 && this._heap[parent].Priority.CompareTo((object) he.Priority) < 0;
                parent = this.GetParent(index))
            {
                this._heap[index] = this._heap[parent];
                index = parent;
            }

            this._heap[index] = he;
        }

        private int GetLeftChild(int index)
        {
            return index * 2 + 1;
        }

        private int GetParent(int index)
        {
            return (index - 1) / 2;
        }

        private void GrowHeap()
        {
            this._capacity = this._capacity * 2 + 1;
            HeapEntry<T>[] heapEntryArray = new HeapEntry<T>[this._capacity];
            Array.Copy((Array) this._heap, 0, (Array) heapEntryArray, 0, this._count);
            this._heap = heapEntryArray;
        }

        private void TrickleDown(int index, HeapEntry<T> he)
        {
            for (int leftChild = this.GetLeftChild(index);
                leftChild < this._count;
                leftChild = this.GetLeftChild(index))
            {
                if (leftChild + 1 < this._count &&
                    this._heap[leftChild].Priority.CompareTo((object) this._heap[leftChild + 1].Priority) < 0)
                    ++leftChild;
                this._heap[index] = this._heap[leftChild];
                index = leftChild;
            }

            this.BubbleUp(index, he);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) new PriorityQueue<T>.PriorityQueueEnumerator(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>) new PriorityQueue<T>.PriorityQueueEnumerator(this);
        }

        public int Count
        {
            get { return this._count; }
        }

        public void CopyTo(Array array, int index)
        {
            Array.Copy((Array) this._heap, 0, array, index, this._count);
        }

        public object SyncRoot
        {
            get { return (object) this; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("capacity", this._capacity);
            info.AddValue("count", this._count);
            HeapEntry<T>[] heapEntryArray = new HeapEntry<T>[this._count];
            Array.Copy((Array) this._heap, 0, (Array) heapEntryArray, 0, this._count);
            info.AddValue("heap", (object) heapEntryArray, typeof(HeapEntry<T>[]));
        }

        [Serializable]
        private class PriorityQueueEnumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private int _index;
            private PriorityQueue<T> _pq;
            private int _version;

            public PriorityQueueEnumerator(PriorityQueue<T> pq)
            {
                this._pq = pq;
                this.Reset();
            }

            private void CheckVersion()
            {
                if (this._version != this._pq._version)
                    throw new InvalidOperationException();
            }

            public void Reset()
            {
                this._index = -1;
                this._version = this._pq._version;
            }

            public T Current
            {
                get
                {
                    this.CheckVersion();
                    return this._pq._heap[this._index].Item;
                }
            }

            public bool MoveNext()
            {
                this.CheckVersion();
                if (this._index + 1 == this._pq._count)
                    return false;
                ++this._index;
                return true;
            }

            public void Dispose()
            {
                this._pq = (PriorityQueue<T>) null;
            }

            object IEnumerator.Current
            {
                get
                {
                    this.CheckVersion();
                    return (object) this._pq._heap[this._index].Item;
                }
            }
        }
    }
}