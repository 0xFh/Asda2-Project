using System.Threading;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Rather performance-hungry message to ensure that a task
    /// executed before continuing
    /// </summary>
    public class WaitRealmMessage : RealmMessage
    {
        private bool m_executed;

        public override void Execute()
        {
            try
            {
                base.Execute();
            }
            finally
            {
                lock (this)
                {
                    this.m_executed = true;
                    Monitor.PulseAll((object) this);
                }
            }
        }

        /// <summary>Waits until this RealmMessage executed.</summary>
        public void Wait()
        {
            if (this.m_executed)
                return;
            lock (this)
                Monitor.Wait((object) this);
        }
    }
}