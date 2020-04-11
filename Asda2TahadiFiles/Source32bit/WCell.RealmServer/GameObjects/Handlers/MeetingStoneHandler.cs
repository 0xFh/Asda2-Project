using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects.Handlers
{
  /// <summary>GO Type 23</summary>
  public class MeetingStoneHandler : GameObjectHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    public override bool Use(Character user)
    {
      GOMeetingStoneEntry entry = (GOMeetingStoneEntry) m_go.Entry;
      if(user == null)
        return false;
      Character character = user;
      Unit target = character.Target;
      if(target == null || target == character || !character.IsAlliedWith(target))
        return false;
      int level = character.Level;
      return level >= entry.MinLevel && level <= entry.MaxLevel;
    }
  }
}