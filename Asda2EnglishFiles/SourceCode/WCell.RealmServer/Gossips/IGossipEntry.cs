namespace WCell.RealmServer.Gossips
{
    /// <summary>
    /// The Id is used by the client to find this entry in its cache.
    /// </summary>
    public interface IGossipEntry
    {
        uint GossipId { get; }

        GossipTextBase[] GossipTexts { get; }

        /// <summary>dynamic gossip entries don't cache their texts</summary>
        bool IsDynamic { get; }
    }
}