namespace WCell.RealmServer.Gossips
{
  /// <summary>
  /// Represents a GossipMenuItem that belongs to a GossipMenu of the given type
  /// </summary>
  public class AssociatedMenuItem<M> : GossipMenuItemBase where M : GossipMenu
  {
    public AssociatedMenuItemAction GetTextCallback { get; set; }

    public AssociatedMenuItemAction GetConfirmTextCallback { get; set; }

    public AssociatedMenuItem(AssociatedMenuItemAction getTextCallback)
      : this(getTextCallback, (convo, menu) => "")
    {
    }

    public AssociatedMenuItem(AssociatedMenuItemAction getTextCallback,
      AssociatedMenuItemAction getConfirmTextCallback)
    {
      GetTextCallback = getTextCallback;
      GetConfirmTextCallback = getConfirmTextCallback;
    }

    public override sealed string GetText(GossipConversation convo)
    {
      return GetTextCallback(convo, (M) convo.CurrentMenu);
    }

    public override string GetConfirmText(GossipConversation convo)
    {
      return GetConfirmTextCallback(convo, (M) convo.CurrentMenu);
    }

    public delegate string AssociatedMenuItemAction(GossipConversation convo, M menu);
  }
}