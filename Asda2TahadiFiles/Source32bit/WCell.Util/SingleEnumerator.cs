using System;
using System.Collections;
using System.Collections.Generic;

namespace WCell.Util
{
  /// <summary>Returns a single element</summary>
  public class SingleEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator where T : class
  {
    private T m_Current;

    public SingleEnumerator(T element)
    {
      Current = element;
    }

    public void Dispose()
    {
      Current = default(T);
    }

    public bool MoveNext()
    {
      return Current != null;
    }

    public void Reset()
    {
      throw new NotImplementedException();
    }

    public T Current
    {
      get
      {
        T current = m_Current;
        m_Current = default(T);
        return current;
      }
      private set { m_Current = value; }
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }
  }
}