using System;

namespace WCell.RealmServer.Entities
{
  public class OneShotObjectUpdateTimer : ObjectUpdateTimer
  {
    public OneShotObjectUpdateTimer(int delayMillis, Action<WorldObject> callback)
    {
      OneShotObjectUpdateTimer objectUpdateTimer = this;
      Delay = delayMillis;
      Callback = obj =>
      {
        callback(obj);
        obj.RemoveUpdateAction(objectUpdateTimer);
      };
    }
  }
}