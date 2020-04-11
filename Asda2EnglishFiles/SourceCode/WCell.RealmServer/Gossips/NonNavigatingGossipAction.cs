namespace WCell.RealmServer.Gossips
{
    public class NonNavigatingGossipAction : IGossipAction
    {
        private GossipActionHandler m_Handler;

        public NonNavigatingGossipAction(GossipActionHandler handler)
        {
            this.m_Handler = handler;
        }

        public GossipActionHandler Handler
        {
            get { return this.m_Handler; }
            set { this.m_Handler = value; }
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
            this.m_Handler(convo);
        }
    }
}