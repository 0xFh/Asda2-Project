namespace WCell.RealmServer.Gossips
{
    /// <summary>
    /// Represents a GossipMenuItem that belongs to a GossipMenu of the given type
    /// </summary>
    public class AssociatedMenuItem<M> : GossipMenuItemBase where M : GossipMenu
    {
        public AssociatedMenuItem<M>.AssociatedMenuItemAction GetTextCallback { get; set; }

        public AssociatedMenuItem<M>.AssociatedMenuItemAction GetConfirmTextCallback { get; set; }

        public AssociatedMenuItem(AssociatedMenuItem<M>.AssociatedMenuItemAction getTextCallback)
            : this(getTextCallback, (AssociatedMenuItem<M>.AssociatedMenuItemAction) ((convo, menu) => ""))
        {
        }

        public AssociatedMenuItem(AssociatedMenuItem<M>.AssociatedMenuItemAction getTextCallback,
            AssociatedMenuItem<M>.AssociatedMenuItemAction getConfirmTextCallback)
        {
            this.GetTextCallback = getTextCallback;
            this.GetConfirmTextCallback = getConfirmTextCallback;
        }

        public override sealed string GetText(GossipConversation convo)
        {
            return this.GetTextCallback(convo, (M) convo.CurrentMenu);
        }

        public override string GetConfirmText(GossipConversation convo)
        {
            return this.GetConfirmTextCallback(convo, (M) convo.CurrentMenu);
        }

        public delegate string AssociatedMenuItemAction(GossipConversation convo, M menu);
    }
}