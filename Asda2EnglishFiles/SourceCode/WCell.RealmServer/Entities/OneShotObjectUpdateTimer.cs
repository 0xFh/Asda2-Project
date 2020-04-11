using System;

namespace WCell.RealmServer.Entities
{
    public class OneShotObjectUpdateTimer : ObjectUpdateTimer
    {
        public OneShotObjectUpdateTimer(int delayMillis, Action<WorldObject> callback)
        {
            OneShotObjectUpdateTimer objectUpdateTimer = this;
            this.Delay = delayMillis;
            this.Callback = (Action<WorldObject>) (obj =>
            {
                callback(obj);
                obj.RemoveUpdateAction((ObjectUpdateTimer) objectUpdateTimer);
            });
        }
    }
}