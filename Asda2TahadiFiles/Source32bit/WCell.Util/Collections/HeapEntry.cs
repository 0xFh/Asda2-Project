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
      _item = item;
      _priority = priority;
    }

    public T Item
    {
      get { return _item; }
    }

    public IComparable Priority
    {
      get { return _priority; }
    }

    public void Clear()
    {
      _item = default(T);
      _priority = null;
    }
  }
}