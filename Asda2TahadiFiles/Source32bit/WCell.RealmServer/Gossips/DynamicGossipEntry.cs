using WCell.Constants.Misc;

namespace WCell.RealmServer.Gossips
{
  public class DynamicGossipEntry : GossipEntry
  {
    public DynamicGossipEntry(uint id, params GossipStringFactory[] texts)
    {
      GossipId = id;
      GossipTexts = new DynamicGossipText[texts.Length];
      float probability = 1f / texts.Length;
      for(int index = 0; index < texts.Length; ++index)
        GossipTexts[index] =
          new DynamicGossipText(texts[index], probability, ChatLanguage.Universal);
    }

    public DynamicGossipEntry(uint id, ChatLanguage lang, params GossipStringFactory[] texts)
    {
      GossipId = id;
      GossipTexts = new DynamicGossipText[texts.Length];
      float probability = 1f / texts.Length;
      for(int index = 0; index < texts.Length; ++index)
        GossipTexts[index] = new DynamicGossipText(texts[index], probability, lang);
    }

    public DynamicGossipEntry(uint id, params DynamicGossipText[] entries)
    {
      GossipId = id;
      GossipTexts = entries;
    }

    public override bool IsDynamic
    {
      get { return true; }
    }

    public DynamicGossipText GetText(int i)
    {
      return (DynamicGossipText) m_Texts[i];
    }
  }
}