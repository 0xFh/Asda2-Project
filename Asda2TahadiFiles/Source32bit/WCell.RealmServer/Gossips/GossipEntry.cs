using WCell.Util.Data;

namespace WCell.RealmServer.Gossips
{
  public abstract class GossipEntry : IGossipEntry
  {
    protected GossipTextBase[] m_Texts;

    public uint GossipId { get; set; }

    public abstract bool IsDynamic { get; }

    /// <summary>
    /// The texts of the StaticGossipEntry DataHolder are actually of type StaticGossipText
    /// </summary>
    [Persistent(8, ActualType = typeof(StaticGossipText))]
    public GossipTextBase[] GossipTexts
    {
      get { return m_Texts; }
      set { m_Texts = value; }
    }

    public uint GetId()
    {
      return GossipId;
    }

    public override string ToString()
    {
      return GetType().Name + " (Id: " + GossipId + ")";
    }
  }
}