using System;

namespace WCell.Core.Timers
{
  /// <summary>
  /// Lightweight timer object that supports one-shot or repeated firing.
  /// </summary>
  /// <remarks>This timer is not standalone, and must be driven via the <see cref="T:WCell.Core.Timers.IUpdatable" /> interface.</remarks>
  public class TimerEntry : IDisposable, IUpdatable
  {
    private int m_millisSinceLastTick;
    public int RemainingInitialDelayMillis;
    public int IntervalMillis;
    public Action<int> Action;

    public TimerEntry()
    {
    }

    /// <summary>
    /// Creates a new timer with the given start delay, interval, and callback.
    /// </summary>
    /// <param name="delay">the delay before firing initially</param>
    /// <param name="intervalMillis">the interval between firing</param>
    /// <param name="callback">the callback to fire</param>
    public TimerEntry(int delay, int intervalMillis, Action<int> callback)
    {
      m_millisSinceLastTick = -1;
      Action = callback;
      RemainingInitialDelayMillis = delay;
      IntervalMillis = intervalMillis;
    }

    public TimerEntry(Action<int> callback)
      : this(0, 0, callback)
    {
    }

    /// <summary>
    /// The amount of time in milliseconds that elapsed between the last timer tick and the last update.
    /// </summary>
    public int MillisSinceLastTick
    {
      get { return m_millisSinceLastTick; }
    }

    /// <summary>Starts the timer.</summary>
    public void Start()
    {
      m_millisSinceLastTick = 0;
    }

    /// <summary>Starts the timer with the given delay.</summary>
    /// <param name="initialDelay">the delay before firing initially</param>
    public void Start(int initialDelay)
    {
      RemainingInitialDelayMillis = initialDelay;
      m_millisSinceLastTick = 0;
    }

    /// <summary>Starts the time with the given delay and interval.</summary>
    /// <param name="initialDelay">the delay before firing initially</param>
    /// <param name="interval">the interval between firing</param>
    public void Start(int initialDelay, int interval)
    {
      RemainingInitialDelayMillis = initialDelay;
      IntervalMillis = interval;
      m_millisSinceLastTick = 0;
    }

    /// <summary>Whether or not the timer is running.</summary>
    public bool IsRunning
    {
      get { return m_millisSinceLastTick >= 0; }
    }

    /// <summary>Stops the timer.</summary>
    public void Stop()
    {
      m_millisSinceLastTick = -1;
    }

    /// <summary>
    /// Updates the timer, firing the callback if enough time has elapsed.
    /// </summary>
    /// <param name="dtMillis">the time change since the last update</param>
    public void Update(int dtMillis)
    {
      if(m_millisSinceLastTick == -1)
        return;
      if(RemainingInitialDelayMillis > 0)
      {
        RemainingInitialDelayMillis -= dtMillis;
        if(RemainingInitialDelayMillis > 0)
          return;
        if(IntervalMillis == 0)
        {
          int millisSinceLastTick = m_millisSinceLastTick;
          Stop();
          Action(millisSinceLastTick);
        }
        else
        {
          Action(m_millisSinceLastTick);
          m_millisSinceLastTick = 0;
        }
      }
      else
      {
        m_millisSinceLastTick += dtMillis;
        if(m_millisSinceLastTick >= IntervalMillis)
        {
          Action(m_millisSinceLastTick);
          if(m_millisSinceLastTick != -1)
            m_millisSinceLastTick -= IntervalMillis;
        }
      }
    }

    /// <summary>Stops and cleans up the timer.</summary>
    public void Dispose()
    {
      Stop();
      Action = null;
    }

    public override bool Equals(object obj)
    {
      if(obj.GetType() != typeof(TimerEntry))
        return false;
      return Equals((TimerEntry) obj);
    }

    public bool Equals(TimerEntry obj)
    {
      return obj.IntervalMillis == IntervalMillis &&
             Equals(obj.Action, Action);
    }

    public override int GetHashCode()
    {
      return IntervalMillis * 397 ^ (Action != null ? Action.GetHashCode() : 0);
    }
  }
}