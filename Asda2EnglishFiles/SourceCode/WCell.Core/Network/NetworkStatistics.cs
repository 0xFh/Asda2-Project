using System;
using WCell.Core.Timers;
using WCell.Util.Collections;

namespace WCell.Core.Network
{
    /// <summary>Manages network-related statistics.</summary>
    public abstract class NetworkStatistics : IUpdatable
    {
        protected TimerEntry m_consumerTimer;
        protected SynchronizedQueue<PacketInfo> m_queuedStats;

        protected NetworkStatistics()
        {
            this.m_consumerTimer = new TimerEntry(new Action<int>(this.ConsumerCallback));
            this.m_queuedStats = new SynchronizedQueue<PacketInfo>();
        }

        public virtual void UpdatePacketInfo(PacketInfo pktInfo)
        {
            this.m_queuedStats.Enqueue(pktInfo);
        }

        protected abstract void ConsumeStatistics();

        protected virtual void ConsumerCallback(int dt)
        {
            this.ConsumeStatistics();
        }

        public void Update(int dt)
        {
            this.m_consumerTimer.Update(dt);
        }
    }
}