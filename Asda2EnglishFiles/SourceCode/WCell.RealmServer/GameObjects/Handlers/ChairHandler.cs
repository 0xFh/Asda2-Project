using NLog;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GOType 7</summary>
    public class ChairHandler : GameObjectHandler
    {
        private static Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>The amount of people currently using this GO</summary>
        public int UserAmount;

        public override bool Use(Character user)
        {
            GOChairEntry entry = this.m_go.Entry as GOChairEntry;
            if (entry.PrivateChair && this.m_go.CreatedBy != EntityId.Zero && user.EntityId != this.m_go.CreatedBy)
                return false;
            if (this.UserAmount < entry.MaxCount)
            {
                ++this.UserAmount;
                user.Map.MoveObject((WorldObject) user, this.m_go.Position);
                user.Orientation = this.m_go.Orientation;
                user.StandState = entry.SitState;
                MovementHandler.SendHeartbeat((Unit) user, this.m_go.Position, this.m_go.Orientation);
            }

            return true;
        }
    }
}