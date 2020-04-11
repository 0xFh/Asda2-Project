using System;
using System.Collections;
using System.Collections.Generic;

namespace WCell.Util
{
    /// <summary>
    /// A simple array that is indexed in a circular fashion.
    /// Elements always start at Tail and end at Head.
    /// 
    /// Tail greater Head:
    /// | x | x | H |   |   |   | T | x | x |
    /// 
    /// Head greater Tail:
    /// |   |   | T | x | x | x | H |   |   |
    /// </summary>
    public class StaticCircularList<T> : IEnumerable<T>, IEnumerable
    {
        private int tail;
        private int head;
        private T[] arr;

        public StaticCircularList(int capacity, Action<T> deleteCallback)
        {
            this.DeleteCallback = deleteCallback;
            this.arr = new T[capacity];
        }

        public Action<T> DeleteCallback { get; set; }

        /// <summary>The index of the tail</summary>
        public int Tail
        {
            get { return this.tail; }
            set { this.tail = value; }
        }

        /// <summary>The index of the head</summary>
        public int Head
        {
            get { return this.head; }
            set { this.head = value; }
        }

        public int Capacity
        {
            get { return this.arr.Length; }
        }

        public int Count
        {
            get { return this.Tail > this.Head ? this.Head + this.Capacity - this.Tail : this.Head - this.Tail; }
        }

        public bool IsFull
        {
            get { return this.Count == this.Capacity; }
        }

        public bool IsEmpty
        {
            get { return this.Tail == this.Head; }
        }

        /// <summary>The Tail item</summary>
        public T TailItem
        {
            get
            {
                if (this.IsEmpty)
                    return default(T);
                return this.arr[this.tail];
            }
        }

        /// <summary>The Head item</summary>
        public T HeadItem
        {
            get
            {
                if (this.IsEmpty)
                    return default(T);
                return this.arr[this.head];
            }
        }

        /// <summary>
        /// Increases the given number by one and wrapping it around, if it's &gt;= Capacity-1
        /// </summary>
        private int IncreaseAndWrap(int num)
        {
            return (num + 1) % this.Capacity;
        }

        private int DecreaseAndWrap(int num)
        {
            return (num - 1) % this.Capacity;
        }

        /// <summary>
        /// Sets the Head item.
        /// Also moves Tail forward, if it was already full, thus replacing the Tail item.
        /// </summary>
        public void Insert(T item)
        {
            if (this.Count + 1 == this.Capacity)
            {
                Action<T> deleteCallback = this.DeleteCallback;
                if (deleteCallback != null)
                    deleteCallback(this.TailItem);
                this.tail = this.IncreaseAndWrap(this.tail);
            }

            this.head = this.IncreaseAndWrap(this.head);
            this.arr[this.head] = item;
        }

        /// <summary>
        /// Iterates over all items, starting at Tail and stopping at Head
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            if (!this.IsEmpty)
            {
                if (this.Tail < this.Head)
                {
                    for (int i = this.Tail; i <= this.Head; ++i)
                        yield return this.arr[i];
                }
                else
                {
                    for (int i = this.Tail; i < this.Capacity; ++i)
                        yield return this.arr[i];
                    for (int i = 0; i <= this.Head; ++i)
                        yield return this.arr[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }
    }
}