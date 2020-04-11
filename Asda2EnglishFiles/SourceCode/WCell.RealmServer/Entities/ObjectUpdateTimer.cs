using System;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
    public class ObjectUpdateTimer
    {
        public ObjectUpdateTimer()
        {
        }

        public ObjectUpdateTimer(int delayMillis, Action<WorldObject> callback)
        {
            this.Callback = callback;
            this.Delay = delayMillis;
            this.LastCallTime = DateTime.Now;
        }

        public DateTime LastCallTime { get; internal set; }

        public Action<WorldObject> Callback { get; set; }

        public int Delay { get; set; }

        public int GetDelayUntilNextExecution(WorldObject obj)
        {
            return this.Delay - (obj.LastUpdateTime - this.LastCallTime).ToMilliSecondsInt();
        }

        public void Execute(WorldObject owner)
        {
            this.Callback(owner);
            this.LastCallTime = owner.LastUpdateTime;
        }

        public override bool Equals(object obj)
        {
            if (obj is ObjectUpdateTimer)
                return this.Callback == ((ObjectUpdateTimer) obj).Callback;
            return false;
        }

        public override int GetHashCode()
        {
            return this.Callback.GetHashCode();
        }
    }
}