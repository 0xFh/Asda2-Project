using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 0: A door.</summary>
    public class DoorHandler : GameObjectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            GOEntry entry = this.m_go.Entry;
            this.m_go.AnimationProgress = this.m_go.AnimationProgress == (byte) 100 ? (byte) 0 : (byte) 100;
            return true;
        }
    }
}