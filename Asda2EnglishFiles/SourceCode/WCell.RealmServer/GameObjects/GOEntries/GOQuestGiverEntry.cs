using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOQuestGiverEntry : GOEntry
    {
        /// <summary>LockId from Lock.dbc</summary>
        public int LockId
        {
            get { return this.Fields[0]; }
        }

        /// <summary>Id of quest-list</summary>
        public uint QuestListId
        {
            get { return (uint) this.Fields[1]; }
        }

        /// <summary>PageId from PageTextMaterial.dbc</summary>
        public int PageTextMaterialId
        {
            get { return this.Fields[2]; }
        }

        /// <summary>Unknown</summary>
        public override uint GossipId
        {
            get { return (uint) this.Fields[3]; }
        }

        /// <summary>Constrained to values 1-4</summary>
        public int CustomAnim
        {
            get { return this.Fields[4]; }
        }

        /// <summary>Unknown</summary>
        public bool NoDamageImmune
        {
            get { return this.Fields[5] > 0; }
        }

        /// <summary>
        /// Reference to Text object containing the text to display upon interacting with this GO (?)
        /// </summary>
        public int OpenTextId
        {
            get { return this.Fields[6]; }
        }

        /// <summary>Unknown</summary>
        public bool Large
        {
            get { return this.Fields[9] > 0; }
        }

        protected internal override void InitEntry()
        {
            this.Lock = LockEntry.Entries.Get<LockEntry>((uint) this.LockId);
            this.LosOk = this.Fields[7] > 0;
            this.AllowMounted = this.Fields[8] > 0;
        }
    }
}