using WCell.Constants.Misc;
using WCell.Util.Data;

namespace WCell.RealmServer.Gossips
{
  public abstract class GossipTextBase
  {
    [NotPersistent]public EmoteType[] Emotes = new EmoteType[6];

    /// <summary>$N = Character name</summary>
    public ChatLanguage Language;

    public float Probability;

    protected GossipTextBase()
    {
    }

    protected GossipTextBase(float probability, ChatLanguage lang = ChatLanguage.Universal)
    {
      Probability = probability;
      Language = lang;
    }

    public EmoteType Emote1
    {
      get { return Emotes[0]; }
      set { Emotes[0] = value; }
    }

    public EmoteType Emote2
    {
      get { return Emotes[1]; }
      set { Emotes[1] = value; }
    }

    public EmoteType Emote3
    {
      get { return Emotes[2]; }
      set { Emotes[2] = value; }
    }

    public EmoteType Emote4
    {
      get { return Emotes[3]; }
      set { Emotes[3] = value; }
    }

    public EmoteType Emote5
    {
      get { return Emotes[4]; }
      set { Emotes[4] = value; }
    }

    public EmoteType Emote6
    {
      get { return Emotes[5]; }
      set { Emotes[5] = value; }
    }

    public abstract string GetTextMale(GossipConversation convo);

    public abstract string GetTextFemale(GossipConversation convo);
  }
}