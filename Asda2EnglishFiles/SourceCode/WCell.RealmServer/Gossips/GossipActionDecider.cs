namespace WCell.RealmServer.Gossips
{
    /// <summary>
    /// Decides whether the item may be displayed/used, in the given convo
    /// </summary>
    public delegate bool GossipActionDecider(GossipConversation convo);
}