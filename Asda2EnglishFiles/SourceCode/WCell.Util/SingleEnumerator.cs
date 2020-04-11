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
            this.Current = element;
        }

        public void Dispose()
        {
            this.Current = default(T);
        }

        public bool MoveNext()
        {
            return (object) this.Current != null;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public T Current
        {
            get
            {
                T current = this.m_Current;
                this.m_Current = default(T);
                return current;
            }
            private set { this.m_Current = value; }
        }

        object IEnumerator.Current
        {
            get { return (object) this.Current; }
        }
    }
}