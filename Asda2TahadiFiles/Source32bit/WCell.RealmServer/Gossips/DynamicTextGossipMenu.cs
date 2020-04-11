using WCell.Constants.Misc;

namespace WCell.RealmServer.Gossips
{
  public abstract class DynamicTextGossipMenu : GossipMenu
  {
    private static readonly DynamicGossipEntry SharedEntry = new DynamicGossipEntry(88993333U,
      new DynamicGossipText(OnTextQuery, 1f,
        ChatLanguage.Universal));

    private static string OnTextQuery(GossipConversation convo)
    {
      return ((DynamicTextGossipMenu) convo.CurrentMenu).GetText(convo);
    }

    protected DynamicTextGossipMenu()
      : base(SharedEntry)
    {
    }

    protected DynamicTextGossipMenu(params GossipMenuItemBase[] items)
      : base(SharedEntry, items)
    {
    }

    public abstract string GetText(GossipConversation convo);
  }
}