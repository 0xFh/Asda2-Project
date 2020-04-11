namespace WCell.Core.Timers
{
  public class TimerRunner
  {
    private readonly TimerBucket[] m_buckets = new TimerBucket[8];
    private float m_totalSeconds;

    public TimerRunner()
    {
      AddBucket(TimerPriority.OneSec, 1f);
      AddBucket(TimerPriority.OnePointFiveSec, 1.5f);
      AddBucket(TimerPriority.FiveSec, 5f);
      AddBucket(TimerPriority.TenSec, 10f);
      AddBucket(TimerPriority.ThirtySec, 30f);
      AddBucket(TimerPriority.OneMin, 60f);
      AddBucket(TimerPriority.OneHour, 3600f);
    }

    private void AddBucket(TimerPriority prio, float delay)
    {
      m_buckets[(int) prio] = new TimerBucket(prio, delay);
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
      get { return m_buckets[(int) priority]; }
    }

    public void Update(float secondsPassed)
    {
      m_totalSeconds += secondsPassed;
      this[TimerPriority.Always].Tick();
      for(int index = 1; index < m_buckets.Length; ++index)
      {
        TimerBucket bucket = m_buckets[index];
        while(bucket.m_LastUpdate + (double) bucket.Delay >= m_totalSeconds)
        {
          bucket.m_LastUpdate += bucket.m_LastUpdate + bucket.Delay - m_totalSeconds;
          bucket.Tick();
        }
      }
    }
  }
}