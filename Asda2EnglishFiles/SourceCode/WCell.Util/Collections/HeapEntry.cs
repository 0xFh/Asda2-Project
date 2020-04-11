using System;

namespace WCell.Util.Collections
{
    [Serializable]
    public struct HeapEntry<T>
    {
        private T _item;
        private IComparable _priority;

        public HeapEntry(T item, IComparable priority)
        {
            this._item = item;
            this._priority = priority;
        }

        public T Item
        {
            get { return this._item; }
        }

        public IComparable Priority
        {
            get { return this._priority; }
        }

        public void Clear()
        {
            this._item = default(T);
            this._priority = (IComparable) null;
        }
    }
}