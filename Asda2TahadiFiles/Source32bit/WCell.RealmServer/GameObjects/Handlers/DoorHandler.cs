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
      GOEntry entry = m_go.Entry;
      m_go.AnimationProgress = m_go.AnimationProgress == (byte) 100 ? (byte) 0 : (byte) 100;
      return true;
    }
  }
}