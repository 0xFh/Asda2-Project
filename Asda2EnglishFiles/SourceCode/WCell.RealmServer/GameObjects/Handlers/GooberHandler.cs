using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 10</summary>
    public class GooberHandler : GameObjectHandler
    {
        public override bool Use(Character user)
        {
            GOGooberEntry entry = (GOGooberEntry) this.m_go.Entry;
            return true;
        }
    }
}