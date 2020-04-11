using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 20</summary>
    public class AuctionHouseHandler : GameObjectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            GOEntry entry = this.m_go.Entry;
            return true;
        }
    }
}