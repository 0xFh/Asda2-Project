using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOGooberEntry : GOEntry
    {
        /// <summary>The Spell associated with this goober.</summary>
        public Spell Spell;

        /// <summary>The LockId from Lock.dbc</summary>
        public int LockId
        {
            get { return this.Fields[0]; }
        }

        /// <summary>
        /// The Id of the quest required to be active in order to interact with this goober.
        /// </summary>
        public override uint QuestId
        {
            get { return (uint) this.Fields[1]; }
        }

        /// <summary>The Id of an Event associated with this goober (?)</summary>
        public int EventId
        {
            get { return this.Fields[2]; }
        }

        /// <summary>
        /// The time delay before this goober auto-closes after being opened. (?)
        /// </summary>
        public int AutoClose
        {
            get { return this.Fields[3]; }
        }

        /// <summary>???</summary>
        public int CustomAnim
        {
            get { return this.Fields[4]; }
        }

        /// <summary>
        /// Time between allowed interactions with this goober (?)
        /// </summary>
        public int Cooldown
        {
            get { return this.Fields[6]; }
        }

        /// <summary>
        /// The Id of a PageText object associated with this goober.
        /// </summary>
        public override uint PageId
        {
            get { return (uint) this.Fields[7]; }
        }

        /// <summary>The LanguageId from Languages.dbc</summary>
        public ChatLanguage LanguageId
        {
            get { return (ChatLanguage) this.Fields[8]; }
        }

        /// <summary>The PageTextMaterialId from PageTextMaterial.dbc</summary>
        public PageMaterial PageTextMaterialId
        {
            get { return (PageMaterial) this.Fields[9]; }
        }

        /// <summary>The SpellId associated with this goober.</summary>
        public SpellId SpellId
        {
            get { return (SpellId) this.Fields[10]; }
        }

        /// <summary>???</summary>
        public bool NoDamageImmune
        {
            get { return this.Fields[11] > 0; }
        }

        /// <summary>???</summary>
        public bool Large
        {
            get { return this.Fields[13] > 0; }
        }

        /// <summary>
        /// The Id of a text object to be displayed when opening this goober (?)
        /// </summary>
        public int OpenTextId
        {
            get { return this.Fields[14]; }
        }

        /// <summary>
        /// The Id of a text object to be displayed when closing this goober (?)
        /// </summary>
        public int CloseTextId
        {
            get { return this.Fields[15]; }
        }

        public int FloatingTooltip
        {
            get { return this.Fields[18]; }
        }

        public override uint GossipId
        {
            get { return (uint) this.Fields[19]; }
        }

        public bool WorldStateSetsState
        {
            get { return this.Fields[20] != 0; }
        }

        protected internal override void InitEntry()
        {
            this.Lock = LockEntry.Entries.Get<LockEntry>((uint) this.LockId);
            this.Spell = SpellHandler.Get(this.SpellId);
            this.IsConsumable = this.Fields[5] > 0;
            this.LinkedTrapId = (uint) this.Fields[12];
            this.LosOk = this.Fields[16] > 0;
            this.AllowMounted = this.Fields[17] > 0;
        }
    }
}