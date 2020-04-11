using System.Collections.Generic;

namespace WCell.Core.Timers
{
    public class TimerBucket
    {
        public readonly TimerPriority Priority;
        public readonly float Delay;
        public readonly ICollection<BucketTimer> Timers;
        internal float m_LastUpdate;

        public TimerBucket(TimerPriority priority, float delay)
        {
            this.Priority = priority;
            this.Delay = delay;
            this.Timers = (ICollection<BucketTimer>) new HashSet<BucketTimer>();
        }

        public float LastUpdate
        {
            get { return this.m_LastUpdate; }
        }

        public void Tick()
        {
            foreach (BucketTimer timer in (IEnumerable<BucketTimer>) this.Timers)
                timer.Action();
        }
    }
}