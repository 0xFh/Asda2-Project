namespace WCell.RealmServer.Gossips
{
    public class NavigatingGossipAction : IGossipAction
    {
        private GossipActionHandler m_Handler;

        public NavigatingGossipAction(GossipActionHandler handler)
        {
            this.m_Handler = handler;
        }

        public GossipActionHandler Handler
        {
            get { return this.m_Handler; }
            set { this.m_Handler = value; }
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
            this.m_Handler(convo);
        }
    }
}