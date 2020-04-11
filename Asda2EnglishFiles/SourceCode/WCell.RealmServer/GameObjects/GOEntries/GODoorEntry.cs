using NLog;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GODoorEntry : GOEntry
    {
        private static Logger sLog = LogManager.GetCurrentClassLogger();

        public int StartOpen
        {
            get { return this.Fields[0]; }
        }

        /// <summary>
        /// LockId from Lock.dbc
        /// "open" in the fields list
        /// </summary>
        public int LockId
        {
            get { return this.Fields[1]; }
        }

        /// <summary>Possibly the time delay before the door closes?</summary>
        public int AutoClose
        {
            get { return this.Fields[2]; }
        }

        public int NoDamageImmune
        {
            get { return this.Fields[3]; }
        }

        /// <summary>
        /// A reference to an object holding the Text to display upon opening the door?
        /// </summary>
        public int OpenTextId
        {
            get { return this.Fields[4]; }
        }

        /// <summary>
        /// A reference to an object holding the Text to display upon closing the door?
        /// </summary>
        public int CloseTextId
        {
            get { return this.Fields[5]; }
        }

        public int IgnoredByPathing
        {
            get { return this.Fields[6]; }
        }

        protected internal override void InitEntry()
        {
            this.Lock = LockEntry.Entries.Get<LockEntry>((uint) this.LockId);
        }
    }
}