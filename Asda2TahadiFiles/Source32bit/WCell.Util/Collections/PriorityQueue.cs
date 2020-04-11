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
      _capacity = 15;
      _heap = new HeapEntry<T>[_capacity];
    }

    protected PriorityQueue(SerializationInfo info, StreamingContext context)
    {
      _capacity = info.GetInt32("capacity");
      _count = info.GetInt32("count");
      HeapEntry<T>[] heapEntryArray = (HeapEntry<T>[]) info.GetValue("heap", typeof(HeapEntry<T>[]));
      _heap = new HeapEntry<T>[_capacity];
      Array.Copy(heapEntryArray, 0, _heap, 0, _count);
      _version = 0;
    }

    public T Dequeue()
    {
      if(_count == 0)
        throw new InvalidOperationException();
      T obj = _heap[0].Item;
      --_count;
      TrickleDown(0, _heap[_count]);
      _heap[_count].Clear();
      ++_version;
      return obj;
    }

    public void Enqueue(T item, IComparable priority)
    {
      if(priority == null)
        throw new ArgumentNullException(nameof(priority));
      if(_count == _capacity)
        GrowHeap();
      ++_count;
      BubbleUp(_count - 1, new HeapEntry<T>(item, priority));
      ++_version;
    }

    private void BubbleUp(int index, HeapEntry<T> he)
    {
      for(int parent = GetParent(index);
        index > 0 && _heap[parent].Priority.CompareTo(he.Priority) < 0;
        parent = GetParent(index))
      {
        _heap[index] = _heap[parent];
        index = parent;
      }

      _heap[index] = he;
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
      _capacity = _capacity * 2 + 1;
      HeapEntry<T>[] heapEntryArray = new HeapEntry<T>[_capacity];
      Array.Copy(_heap, 0, heapEntryArray, 0, _count);
      _heap = heapEntryArray;
    }

    private void TrickleDown(int index, HeapEntry<T> he)
    {
      for(int leftChild = GetLeftChild(index);
        leftChild < _count;
        leftChild = GetLeftChild(index))
      {
        if(leftChild + 1 < _count &&
           _heap[leftChild].Priority.CompareTo(_heap[leftChild + 1].Priority) < 0)
          ++leftChild;
        _heap[index] = _heap[leftChild];
        index = leftChild;
      }

      BubbleUp(index, he);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new PriorityQueueEnumerator(this);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return new PriorityQueueEnumerator(this);
    }

    public int Count
    {
      get { return _count; }
    }

    public void CopyTo(Array array, int index)
    {
      Array.Copy(_heap, 0, array, index, _count);
    }

    public object SyncRoot
    {
      get { return this; }
    }

    public bool IsSynchronized
    {
      get { return false; }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("capacity", _capacity);
      info.AddValue("count", _count);
      HeapEntry<T>[] heapEntryArray = new HeapEntry<T>[_count];
      Array.Copy(_heap, 0, heapEntryArray, 0, _count);
      info.AddValue("heap", heapEntryArray, typeof(HeapEntry<T>[]));
    }

    [Serializable]
    private class PriorityQueueEnumerator : IEnumerator<T>, IDisposable, IEnumerator
    {
      private int _index;
      private PriorityQueue<T> _pq;
      private int _version;

      public PriorityQueueEnumerator(PriorityQueue<T> pq)
      {
        _pq = pq;
        Reset();
      }

      private void CheckVersion()
      {
        if(_version != _pq._version)
          throw new InvalidOperationException();
      }

      public void Reset()
      {
        _index = -1;
        _version = _pq._version;
      }

      public T Current
      {
        get
        {
          CheckVersion();
          return _pq._heap[_index].Item;
        }
      }

      public bool MoveNext()
      {
        CheckVersion();
        if(_index + 1 == _pq._count)
          return false;
        ++_index;
        return true;
      }

      public void Dispose()
      {
        _pq = null;
      }

      object IEnumerator.Current
      {
        get
        {
          CheckVersion();
          return _pq._heap[_index].Item;
        }
      }
    }
  }
}