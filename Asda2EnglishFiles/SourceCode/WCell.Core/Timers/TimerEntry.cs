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
        public System.Action<int> Action;

        public TimerEntry()
        {
        }

        /// <summary>
        /// Creates a new timer with the given start delay, interval, and callback.
        /// </summary>
        /// <param name="delay">the delay before firing initially</param>
        /// <param name="intervalMillis">the interval between firing</param>
        /// <param name="callback">the callback to fire</param>
        public TimerEntry(int delay, int intervalMillis, System.Action<int> callback)
        {
            this.m_millisSinceLastTick = -1;
            this.Action = callback;
            this.RemainingInitialDelayMillis = delay;
            this.IntervalMillis = intervalMillis;
        }

        public TimerEntry(System.Action<int> callback)
            : this(0, 0, callback)
        {
        }

        /// <summary>
        /// The amount of time in milliseconds that elapsed between the last timer tick and the last update.
        /// </summary>
        public int MillisSinceLastTick
        {
            get { return this.m_millisSinceLastTick; }
        }

        /// <summary>Starts the timer.</summary>
        public void Start()
        {
            this.m_millisSinceLastTick = 0;
        }

        /// <summary>Starts the timer with the given delay.</summary>
        /// <param name="initialDelay">the delay before firing initially</param>
        public void Start(int initialDelay)
        {
            this.RemainingInitialDelayMillis = initialDelay;
            this.m_millisSinceLastTick = 0;
        }

        /// <summary>Starts the time with the given delay and interval.</summary>
        /// <param name="initialDelay">the delay before firing initially</param>
        /// <param name="interval">the interval between firing</param>
        public void Start(int initialDelay, int interval)
        {
            this.RemainingInitialDelayMillis = initialDelay;
            this.IntervalMillis = interval;
            this.m_millisSinceLastTick = 0;
        }

        /// <summary>Whether or not the timer is running.</summary>
        public bool IsRunning
        {
            get { return this.m_millisSinceLastTick >= 0; }
        }

        /// <summary>Stops the timer.</summary>
        public void Stop()
        {
            this.m_millisSinceLastTick = -1;
        }

        /// <summary>
        /// Updates the timer, firing the callback if enough time has elapsed.
        /// </summary>
        /// <param name="dtMillis">the time change since the last update</param>
        public void Update(int dtMillis)
        {
            if (this.m_millisSinceLastTick == -1)
                return;
            if (this.RemainingInitialDelayMillis > 0)
            {
                this.RemainingInitialDelayMillis -= dtMillis;
                if (this.RemainingInitialDelayMillis > 0)
                    return;
                if (this.IntervalMillis == 0)
                {
                    int millisSinceLastTick = this.m_millisSinceLastTick;
                    this.Stop();
                    this.Action(millisSinceLastTick);
                }
                else
                {
                    this.Action(this.m_millisSinceLastTick);
                    this.m_millisSinceLastTick = 0;
                }
            }
            else
            {
                this.m_millisSinceLastTick += dtMillis;
                if (this.m_millisSinceLastTick >= this.IntervalMillis)
                {
                    this.Action(this.m_millisSinceLastTick);
                    if (this.m_millisSinceLastTick != -1)
                        this.m_millisSinceLastTick -= this.IntervalMillis;
                }
            }
        }

        /// <summary>Stops and cleans up the timer.</summary>
        public void Dispose()
        {
            this.Stop();
            this.Action = (System.Action<int>) null;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(TimerEntry))
                return false;
            return this.Equals((TimerEntry) obj);
        }

        public bool Equals(TimerEntry obj)
        {
            return obj.IntervalMillis == this.IntervalMillis &&
                   object.Equals((object) obj.Action, (object) this.Action);
        }

        public override int GetHashCode()
        {
            return this.IntervalMillis * 397 ^ (this.Action != null ? this.Action.GetHashCode() : 0);
        }
    }
}