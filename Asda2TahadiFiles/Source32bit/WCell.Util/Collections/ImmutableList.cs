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
      list = new List<T>(capacity);
    }

    public ImmutableList(IEnumerable<T> collection)
    {
      list = new List<T>(collection);
    }

    public void Add(T item)
    {
      List<T> objList = CopyList();
      objList.Add(item);
      list = objList;
    }

    public void Clear()
    {
      list = new List<T>();
    }

    public bool Contains(T item)
    {
      return list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      List<T> objList = CopyList();
      objList.CopyTo(array, arrayIndex);
      list = objList;
    }

    public bool Remove(T item)
    {
      List<T> objList = CopyList();
      bool flag = objList.Remove(item);
      list = objList;
      return flag;
    }

    public int Count
    {
      get { return list.Count; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public int IndexOf(T item)
    {
      return list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      List<T> objList = CopyList();
      objList.Insert(index, item);
      list = objList;
    }

    public void RemoveAt(int index)
    {
      List<T> objList = CopyList();
      objList.RemoveAt(index);
      list = objList;
    }

    public T this[int index]
    {
      get { return list[index]; }
      set { list[index] = value; }
    }

    public IEnumerator<T> GetEnumerator()
    {
      return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private List<T> CopyList()
    {
      return list.ToList();
    }
  }
}