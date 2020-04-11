namespace WCell.RealmServer.Gossips
{
    /// <summary>
    /// A class determining whether an item should be shown to specific character and specifying reaction on
    /// item selection
    /// </summary>
    public interface IGossipAction
    {
        /// <summary>
        /// Whether this Action determines the next Action.
        /// False, if it this Action does not send a GossipMenu.
        /// True, if this Action determines the next GossipMenu to be sent, itself.
        /// </summary>
        bool Navigates { get; }

        /// <summary>
        /// Should item be shown to and used by this conversation's character?
        /// </summary>
        /// <returns>true if yes</returns>
        bool CanUse(GossipConversation convo);

        /// <summary>Handler of item selection</summary>
        void OnSelect(GossipConversation convo);
    }
}