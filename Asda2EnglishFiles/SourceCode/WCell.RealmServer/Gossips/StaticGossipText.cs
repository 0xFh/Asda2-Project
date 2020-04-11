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
            this.TextMale = this.TextFemale = text;
        }

        public override string GetTextMale(GossipConversation convo)
        {
            return this.TextMale;
        }

        public override string GetTextFemale(GossipConversation convo)
        {
            return this.TextFemale;
        }

        public override string ToString()
        {
            return "Text: " + this.TextFemale;
        }
    }
}