using System.Threading;
using WCell.Util.Collections;

namespace WCell.Core.Database
{
    public class SingleStringMover
    {
        private SynchronizedQueue<string> q = new SynchronizedQueue<string>();

        public virtual string Read()
        {
            while (this.q.Count == 0)
            {
                lock (this.q)
                    Monitor.Wait((object) this.q);
            }

            return this.q.Dequeue();
        }

        public virtual void Write(string s)
        {
            this.q.Enqueue(s);
            lock (this.q)
                Monitor.Pulse((object) this.q);
        }
    }
}