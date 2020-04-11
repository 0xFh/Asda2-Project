using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 31: Instance Portals (apparently)</summary>
    public class DungeonDifficultyHandler : GameObjectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            GOEntry entry = this.m_go.Entry;
            return true;
        }
    }
}