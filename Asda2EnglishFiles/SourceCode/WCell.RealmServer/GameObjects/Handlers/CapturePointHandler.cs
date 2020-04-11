using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 29</summary>
    public class CapturePointHandler : GameObjectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            GOCapturePointEntry entry = (GOCapturePointEntry) this.m_go.Entry;
            return true;
        }
    }
}