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
      GOChairEntry entry = m_go.Entry as GOChairEntry;
      if(entry.PrivateChair && m_go.CreatedBy != EntityId.Zero && user.EntityId != m_go.CreatedBy)
        return false;
      if(UserAmount < entry.MaxCount)
      {
        ++UserAmount;
        user.Map.MoveObject(user, m_go.Position);
        user.Orientation = m_go.Orientation;
        user.StandState = entry.SitState;
        MovementHandler.SendHeartbeat(user, m_go.Position, m_go.Orientation);
      }

      return true;
    }
  }
}