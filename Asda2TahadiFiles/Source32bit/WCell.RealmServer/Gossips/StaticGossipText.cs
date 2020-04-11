using WCell.Constants.Misc;

namespace WCell.RealmServer.Gossips
{
  public class StaticGossipText : GossipTextBase
  {
    public string TextMale;
    public string TextFemale;

    public StaticGossipText()
    {
    }

    public StaticGossipText(string text, float probability, ChatLanguage lang = ChatLanguage.Universal)
      : base(probability, lang)
    {
      TextMale = TextFemale = text;
    }

    public override string GetTextMale(GossipConversation convo)
    {
      return TextMale;
    }

    public override string GetTextFemale(GossipConversation convo)
    {
      return TextFemale;
    }

    public override string ToString()
    {
      return "Text: " + TextFemale;
    }
  }
}