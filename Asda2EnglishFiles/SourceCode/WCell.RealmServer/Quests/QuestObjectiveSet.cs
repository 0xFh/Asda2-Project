using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
    public class QuestObjectiveSet
    {
        [NotPersistent] public string[] Texts = new string[4];

        public string Text1
        {
            get { return this.Texts[0]; }
            set { this.Texts[0] = value; }
        }

        public string Text2
        {
            get { return this.Texts[1]; }
            set { this.Texts[1] = value; }
        }

        public string Text3
        {
            get { return this.Texts[2]; }
            set { this.Texts[2] = value; }
        }

        public string Text4
        {
            get { return this.Texts[3]; }
            set { this.Texts[3] = value; }
        }
    }
}