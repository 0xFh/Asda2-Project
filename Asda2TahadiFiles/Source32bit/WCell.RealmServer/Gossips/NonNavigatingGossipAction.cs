namespace WCell.RealmServer.Gossips
{
  public class NonNavigatingGossipAction : IGossipAction
  {
    private GossipActionHandler m_Handler;

    public NonNavigatingGossipAction(GossipActionHandler handler)
    {
      m_Handler = handler;
    }

    public GossipActionHandler Handler
    {
      get { return m_Handler; }
      set { m_Handler = value; }
    }

    public virtual bool Navigates
    {
      get { return false; }
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