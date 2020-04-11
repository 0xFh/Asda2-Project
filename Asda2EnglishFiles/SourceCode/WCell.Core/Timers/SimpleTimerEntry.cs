using System;

namespace WCell.Core.Timers
{
    /// <summary>
    /// New even more lightweight Timer class to replace the old TimerEntry class
    /// </summary>
    public class SimpleTimerEntry
    {
        /// <summary>Whether this is a one-shot timer</summary>
        public readonly bool IsOneShot;

        internal SimpleTimerEntry(int delayMillis, Action callback, long time, bool isOneShot)
        {
            this.Callback = callback;
            this.Delay = delayMillis;
            this.LastCallTime = time;
            this.IsOneShot = isOneShot;
        }

        public long LastCallTime { get; private set; }

        public Action Callback { get; set; }

        public int Delay { get; set; }

        internal void Execute(SelfRunningTaskQueue queue)
        {
            this.Callback();
            this.LastCallTime = queue.LastUpdateTime;
            if (!this.IsOneShot)
                return;
            queue.CancelTimer(this);
        }

        public override bool Equals(object obj)
        {
            return obj is SimpleTimerEntry && this.Callback == ((SimpleTimerEntry) obj).Callback;
        }

        public override int GetHashCode()
        {
            return this.Callback.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} (Callback = {1}, Delay = {2})", (object) this.GetType(), (object) this.Callback,
                (object) this.Delay);
        }
    }
}