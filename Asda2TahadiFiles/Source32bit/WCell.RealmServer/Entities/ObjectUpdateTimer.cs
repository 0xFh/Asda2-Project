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
      Callback = callback;
      Delay = delayMillis;
      LastCallTime = DateTime.Now;
    }

    public DateTime LastCallTime { get; internal set; }

    public Action<WorldObject> Callback { get; set; }

    public int Delay { get; set; }

    public int GetDelayUntilNextExecution(WorldObject obj)
    {
      return Delay - (obj.LastUpdateTime - LastCallTime).ToMilliSecondsInt();
    }

    public void Execute(WorldObject owner)
    {
      Callback(owner);
      LastCallTime = owner.LastUpdateTime;
    }

    public override bool Equals(object obj)
    {
      if(obj is ObjectUpdateTimer)
        return Callback == ((ObjectUpdateTimer) obj).Callback;
      return false;
    }

    public override int GetHashCode()
    {
      return Callback.GetHashCode();
    }
  }
}