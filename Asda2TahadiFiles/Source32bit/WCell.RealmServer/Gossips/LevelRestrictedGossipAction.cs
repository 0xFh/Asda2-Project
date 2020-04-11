namespace WCell.RealmServer.Gossips
{
  /// <summary>
  /// Gossip action. Allows showing of the gossip item only to characters with level higher than specified
  /// </summary>
  public class LevelRestrictedGossipAction : NonNavigatingGossipAction
  {
    private readonly uint m_level;

    public LevelRestrictedGossipAction(uint level, GossipActionHandler handler)
      : base(handler)
    {
      m_level = level;
    }

    public override bool CanUse(GossipConversation convo)
    {
      return convo.Character.Level >= m_level;
    }
  }
}