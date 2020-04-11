using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 4</summary>
    public class BinderHandler : GameObjectHandler
    {
        private static Logger sLog = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            return true;
        }
    }
}