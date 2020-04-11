using WCell.Constants.Misc;

namespace WCell.RealmServer.Gossips
{
    public class DynamicGossipEntry : GossipEntry
    {
        public DynamicGossipEntry(uint id, params GossipStringFactory[] texts)
        {
            this.GossipId = id;
            this.GossipTexts = (GossipTextBase[]) new DynamicGossipText[texts.Length];
            float probability = 1f / (float) texts.Length;
            for (int index = 0; index < texts.Length; ++index)
                this.GossipTexts[index] =
                    (GossipTextBase) new DynamicGossipText(texts[index], probability, ChatLanguage.Universal);
        }

        public DynamicGossipEntry(uint id, ChatLanguage lang, params GossipStringFactory[] texts)
        {
            this.GossipId = id;
            this.GossipTexts = (GossipTextBase[]) new DynamicGossipText[texts.Length];
            float probability = 1f / (float) texts.Length;
            for (int index = 0; index < texts.Length; ++index)
                this.GossipTexts[index] = (GossipTextBase) new DynamicGossipText(texts[index], probability, lang);
        }

        public DynamicGossipEntry(uint id, params DynamicGossipText[] entries)
        {
            this.GossipId = id;
            this.GossipTexts = (GossipTextBase[]) entries;
        }

        public override bool IsDynamic
        {
            get { return true; }
        }

        public DynamicGossipText GetText(int i)
        {
            return (DynamicGossipText) this.m_Texts[i];
        }
    }
}