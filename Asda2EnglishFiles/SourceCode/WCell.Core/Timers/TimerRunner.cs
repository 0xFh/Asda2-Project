namespace WCell.Core.Timers
{
    public class TimerRunner
    {
        private readonly TimerBucket[] m_buckets = new TimerBucket[8];
        private float m_totalSeconds;

        public TimerRunner()
        {
            this.AddBucket(TimerPriority.OneSec, 1f);
            this.AddBucket(TimerPriority.OnePointFiveSec, 1.5f);
            this.AddBucket(TimerPriority.FiveSec, 5f);
            this.AddBucket(TimerPriority.TenSec, 10f);
            this.AddBucket(TimerPriority.ThirtySec, 30f);
            this.AddBucket(TimerPriority.OneMin, 60f);
            this.AddBucket(TimerPriority.OneHour, 3600f);
        }

        private void AddBucket(TimerPriority prio, float delay)
        {
            this.m_buckets[(int) prio] = new TimerBucket(prio, delay);
        }

        public void AddOneShot(BucketTimer timer, uint millis)
        {
        }

        public void AddPeriodic(BucketTimer timer, uint millis)
        {
        }

        public void Remove(BucketTimer timer)
        {
            this[timer.priority].Timers.Remove(timer);
        }

        public TimerBucket this[TimerPriority priority]
        {
            get { return this.m_buckets[(int) priority]; }
        }

        public void Update(float secondsPassed)
        {
            this.m_totalSeconds += secondsPassed;
            this[TimerPriority.Always].Tick();
            for (int index = 1; index < this.m_buckets.Length; ++index)
            {
                TimerBucket bucket = this.m_buckets[index];
                while ((double) bucket.m_LastUpdate + (double) bucket.Delay >= (double) this.m_totalSeconds)
                {
                    bucket.m_LastUpdate += bucket.m_LastUpdate + bucket.Delay - this.m_totalSeconds;
                    bucket.Tick();
                }
            }
        }
    }
}