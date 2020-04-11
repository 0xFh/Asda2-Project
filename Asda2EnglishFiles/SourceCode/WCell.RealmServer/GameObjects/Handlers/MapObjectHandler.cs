using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 14</summary>
    public class MapObjectHandler : GameObjectHandler
    {
        private static Logger sLog = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            return true;
        }
    }
}