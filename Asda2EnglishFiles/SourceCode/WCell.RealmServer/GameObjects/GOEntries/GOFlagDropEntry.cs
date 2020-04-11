using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOFlagDropEntry : GOFlagEntry
    {
        /// <summary>
        /// Id for an Event that is triggered upon activating this object (?)
        /// </summary>
        public int EventId
        {
            get { return this.Fields[1]; }
        }

        /// <summary>SpellId from Spells.dbc</summary>
        public override SpellId PickupSpellId
        {
            get { return (SpellId) this.Fields[2]; }
        }

        /// <summary>???</summary>
        public override bool NoDamageImmune
        {
            get { return this.Fields[3] != 0; }
        }

        /// <summary>
        /// Id for a text object that is displayed when activating this object (?)
        /// </summary>
        public override int OpenTextId
        {
            get { return this.Fields[4]; }
        }

        protected internal override void InitEntry()
        {
            this.Lock = LockEntry.Entries.Get<LockEntry>((uint) this.LockId);
        }
    }
}