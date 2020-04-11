using WCell.Constants.Misc;

namespace WCell.RealmServer.Gossips
{
  public class DynamicGossipText : GossipTextBase
  {
    public DynamicGossipText(GossipStringFactory stringGetter, float probability = 1f,
      ChatLanguage lang = ChatLanguage.Universal)
      : base(probability, lang)
    {
      StringGetter = stringGetter;
    }

    public GossipStringFactory StringGetter { get; set; }

    public override string GetTextMale(GossipConversation convo)
    {
      if(convo == null)
        return "<invalid context>";
      return GetTextFemale(convo);
    }

    public override string GetTextFemale(GossipConversation convo)
    {
      if(convo == null)
        return "<invalid context>";
      return StringGetter(convo);
    }
  }
}