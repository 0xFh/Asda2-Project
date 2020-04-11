using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WCell.Util.Collections
{
  /// <summary>
  /// Represents a lock-free, thread-safe, first-in, first-out collection of objects.
  /// </summary>
  /// <typeparam name="T">specifies the type of the elements in the queue</typeparam>
  /// <remarks>Enumeration and clearing are not thread-safe.</remarks>
  public class LockfreeQueue<T> : IEnumerable<T>, IEnumerable
  {
    private SingleLinkNode<T> _head;
    private SingleLinkNode<T> _tail;
    private int _count;

    /// <summary>Default constructor.</summary>
    public LockfreeQueue()
    {
      _head = new SingleLinkNode<T>();
      _tail = _head;
    }

    public LockfreeQueue(IEnumerable<T> items)
      : this()
    {
      foreach(T obj in items)
        Enqueue(obj);
    }

    /// <summary>Gets the number of elements contained in the queue.</summary>
    public int Count
    {
      get { return Thread.VolatileRead(ref _count); }
    }

    /// <summary>Adds an object to the end of the queue.</summary>
    /// <param name="item">the object to add to the queue</param>
    public void Enqueue(T item)
    {
      SingleLinkNode<T> comparand = null;
      SingleLinkNode<T> singleLinkNode = new SingleLinkNode<T>
      {
        Item = item
      };
      bool flag = false;
      while(!flag)
      {
        comparand = _tail;
        SingleLinkNode<T> next = comparand.Next;
        if(_tail == comparand)
        {
          if(next == null)
            flag = Interlocked.CompareExchange(ref _tail.Next, singleLinkNode,
                     null) == null;
          else
            Interlocked.CompareExchange(ref _tail, next, comparand);
        }
      }

      Interlocked.CompareExchange(ref _tail, singleLinkNode, comparand);
      Interlocked.Increment(ref _count);
    }

    public T TryDequeue()
    {
      T obj;
      TryDequeue(out obj);
      return obj;
    }

    /// <summary>
    /// Removes and returns the object at the beginning of the queue.
    /// </summary>
    /// <param name="item">
    /// when the method returns, contains the object removed from the beginning of the queue,
    /// if the queue is not empty; otherwise it is the default value for the element type
    /// </param>
    /// <returns>
    /// true if an object from removed from the beginning of the queue;
    /// false if the queue is empty
    /// </returns>
    public bool TryDequeue(out T item)
    {
      item = default(T);
      bool flag = false;
      while(!flag)
      {
        SingleLinkNode<T> head = _head;
        SingleLinkNode<T> tail = _tail;
        SingleLinkNode<T> next = head.Next;
        if(head == _head)
        {
          if(head == tail)
          {
            if(next == null)
              return false;
            Interlocked.CompareExchange(ref _tail, next, tail);
          }
          else
          {
            item = next.Item;
            flag = Interlocked.CompareExchange(ref _head, next, head) == head;
          }
        }
      }

      Interlocked.Decrement(ref _count);
      return true;
    }

    /// <summary>
    /// Removes and returns the object at the beginning of the queue.
    /// </summary>
    /// <returns>the object that is removed from the beginning of the queue</returns>
    public T Dequeue()
    {
      T obj;
      if(!TryDequeue(out obj))
        throw new InvalidOperationException("the queue is empty");
      return obj;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>an enumerator for the queue</returns>
    public IEnumerator<T> GetEnumerator()
    {
      SingleLinkNode<T> currentNode = _head;
      while(currentNode.Item != null)
      {
        yield return currentNode.Item;
        if((currentNode = currentNode.Next) == null)
          break;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>Clears the queue.</summary>
    /// <remarks>This method is not thread-safe.</remarks>
    public void Clear()
    {
      SingleLinkNode<T> singleLinkNode1 = _head;
      while(singleLinkNode1 != null)
      {
        SingleLinkNode<T> singleLinkNode2 = singleLinkNode1;
        singleLinkNode1 = singleLinkNode1.Next;
        singleLinkNode2.Item = default(T);
        singleLinkNode2.Next = null;
      }

      _head = new SingleLinkNode<T>();
      _tail = _head;
      _count = 0;
    }
  }
}