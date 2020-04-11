using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOFlagStandEntry : GOFlagEntry
    {
        /// <summary>SpellId from Spell.dbc</summary>
        public override SpellId PickupSpellId
        {
            get { return (SpellId) this.Fields[1]; }
        }

        /// <summary>Activation radius (?)</summary>
        public int Radius
        {
            get { return this.Fields[2]; }
        }

        /// <summary>SpellId from Spells.dbc</summary>
        public int ReturnAuraId
        {
            get { return this.Fields[3]; }
        }

        public override bool NoDamageImmune
        {
            get { return this.Fields[5] != 0; }
        }

        /// <summary>SpellId from Spells.dbc</summary>
        public SpellId ReturnSpellId
        {
            get { return (SpellId) this.Fields[4]; }
        }

        /// <summary>
        /// Id of a text object that is shown when the object is activated (?)
        /// </summary>
        public override int OpenTextId
        {
            get { return this.Fields[6]; }
        }

        protected internal override void InitEntry()
        {
            this.Lock = LockEntry.Entries.Get<LockEntry>((uint) this.LockId);
            this.LosOk = this.Fields[7] != 0;
        }
    }
}