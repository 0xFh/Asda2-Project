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
            get { return this.m_Texts; }
            set { this.m_Texts = value; }
        }

        public uint GetId()
        {
            return this.GossipId;
        }

        public override string ToString()
        {
            return this.GetType().Name + " (Id: " + (object) this.GossipId + ")";
        }
    }
}