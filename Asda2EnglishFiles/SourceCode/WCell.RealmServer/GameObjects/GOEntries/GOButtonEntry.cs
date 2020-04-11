using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOButtonEntry : GOEntry
    {
        /// <summary>Possibly whether or not this button is pressed?</summary>
        public int StartOpen
        {
            get { return this.Fields[0]; }
        }

        /// <summary>LockId from Lock.dbc</summary>
        public int LockId
        {
            get { return this.Fields[1]; }
        }

        /// <summary>Possibly the time delay before the door auto-closes?</summary>
        public int AutoClose
        {
            get { return this.Fields[2]; }
        }

        /// <summary>???</summary>
        public int NoDamageImmune
        {
            get { return this.Fields[4]; }
        }

        /// <summary>???</summary>
        public int Large
        {
            get { return this.Fields[5]; }
        }

        /// <summary>
        /// A reference to an object holding the Text to display upon pressing the button?
        /// </summary>
        public int OpenTextId
        {
            get { return this.Fields[6]; }
        }

        /// <summary>
        /// A reference to an object holding the Text to display upon unpressing the button?
        /// </summary>
        public int CloseTextId
        {
            get { return this.Fields[7]; }
        }

        protected internal override void InitEntry()
        {
            this.Lock = LockEntry.Entries.Get<LockEntry>((uint) this.LockId);
            this.LinkedTrapId = (uint) this.Fields[3];
            this.LosOk = this.Fields[8] == 1;
        }
    }
}