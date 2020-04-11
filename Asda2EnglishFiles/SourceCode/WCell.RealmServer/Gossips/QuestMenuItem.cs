using System;

namespace WCell.RealmServer.Gossips
{
    /// <summary>Represents quest item in menu</summary>
    [Serializable]
    public class QuestMenuItem
    {
        public uint ID;

        /// <summary>
        /// 2 = Available
        /// 4 = Anything else?
        /// </summary>
        public uint Status;

        public uint Level;
        public string Text;

        public QuestMenuItem()
        {
            this.Text = string.Empty;
        }

        public QuestMenuItem(uint id, uint status, uint level, string text)
        {
            this.ID = id;
            this.Status = status;
            this.Text = text;
            this.Level = level;
        }
    }
}