namespace WCell.RealmServer.Gossips
{
  public class NavigatingGossipAction : IGossipAction
  {
    private GossipActionHandler m_Handler;

    public NavigatingGossipAction(GossipActionHandler handler)
    {
      m_Handler = handler;
    }

    public GossipActionHandler Handler
    {
      get { return m_Handler; }
      set { m_Handler = value; }
    }

    public bool Navigates
    {
      get { return true; }
    }

    public virtual bool CanUse(GossipConversation convo)
    {
      return true;
    }

    public void OnSelect(GossipConversation convo)
    {
      m_Handler(convo);
    }
  }
}