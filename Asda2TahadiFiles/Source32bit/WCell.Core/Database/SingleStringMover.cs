using System.Threading;
using WCell.Util.Collections;

namespace WCell.Core.Database
{
  public class SingleStringMover
  {
    private SynchronizedQueue<string> q = new SynchronizedQueue<string>();

    public virtual string Read()
    {
      while(q.Count == 0)
      {
        lock(q)
          Monitor.Wait(q);
      }

      return q.Dequeue();
    }

    public virtual void Write(string s)
    {
      q.Enqueue(s);
      lock(q)
        Monitor.Pulse(q);
    }
  }
}