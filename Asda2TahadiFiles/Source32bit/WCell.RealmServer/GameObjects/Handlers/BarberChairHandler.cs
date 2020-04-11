using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects.Handlers
{
  public class BarberChairHandler : GameObjectHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    public override bool Use(Character user)
    {
      if(m_go.Entry is GOBarberChairEntry)
      {
        GOBarberChairEntry entry = m_go.Entry as GOBarberChairEntry;
        if(user == null)
          return false;
        Character character = user;
        character.Orientation = m_go.Orientation;
        character.TeleportTo(m_go);
        character.StandState = entry.SitState;
      }
      else
        log.Error(
          "BarberChairHandler: Incorrect underlying Entry type: ({0}) for this handler.",
          m_go.Entry);

      return true;
    }
  }
}