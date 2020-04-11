using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WCell.Util.Collections
{
    public class ImmutableList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private List<T> list;

        public ImmutableList()
            : this(10)
        {
        }

        public ImmutableList(int capacity)
        {
            this.list = new List<T>(capacity);
        }

        public ImmutableList(IEnumerable<T> collection)
        {
            this.list = new List<T>(collection);
        }

        public void Add(T item)
        {
            List<T> objList = this.CopyList();
            objList.Add(item);
            this.list = objList;
        }

        public void Clear()
        {
            this.list = new List<T>();
        }

        public bool Contains(T item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            List<T> objList = this.CopyList();
            objList.CopyTo(array, arrayIndex);
            this.list = objList;
        }

        public bool Remove(T item)
        {
            List<T> objList = this.CopyList();
            bool flag = objList.Remove(item);
            this.list = objList;
            return flag;
        }

        public int Count
        {
            get { return this.list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            return this.list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            List<T> objList = this.CopyList();
            objList.Insert(index, item);
            this.list = objList;
        }

        public void RemoveAt(int index)
        {
            List<T> objList = this.CopyList();
            objList.RemoveAt(index);
            this.list = objList;
        }

        public T this[int index]
        {
            get { return this.list[index]; }
            set { this.list[index] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>) this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }

        private List<T> CopyList()
        {
            return this.list.ToList<T>();
        }
    }
}