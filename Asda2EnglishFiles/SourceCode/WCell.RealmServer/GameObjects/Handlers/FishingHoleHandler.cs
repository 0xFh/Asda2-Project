using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 25</summary>
    public class FishingHoleHandler : GameObjectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            GOEntry entry = this.m_go.Entry;
            return true;
        }
    }
}