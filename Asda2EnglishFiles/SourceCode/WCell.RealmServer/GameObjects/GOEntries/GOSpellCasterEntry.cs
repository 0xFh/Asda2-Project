using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOSpellCasterEntry : GOEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();
        public Spell Spell;

        /// <summary>The SpellId</summary>
        public SpellId SpellId
        {
            get { return (SpellId) this.Fields[0]; }
        }

        /// <summary>The number of times this can cast the Spell</summary>
        public int Charges
        {
            get
            {
                if (this.Fields[1] != 0)
                    return this.Fields[1];
                return 1;
            }
        }

        /// <summary>
        /// Whether you must be in the same group as the caster to recieve the Spell effect.
        /// </summary>
        public override bool IsPartyOnly
        {
            get { return this.Fields[2] > 0; }
        }

        public bool Large
        {
            get { return this.Fields[4] != 0; }
        }

        protected internal override void InitEntry()
        {
            this.Spell = SpellHandler.Get(this.SpellId);
            this.AllowMounted = this.Fields[3] != 0;
        }
    }
}