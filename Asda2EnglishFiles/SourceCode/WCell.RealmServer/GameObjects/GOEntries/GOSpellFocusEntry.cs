using WCell.Constants.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOSpellFocusEntry : GOEntry
    {
        /// <summary>The type of SpellFocus this is.</summary>
        public SpellFocus SpellFocus
        {
            get { return (SpellFocus) this.Fields[0]; }
        }

        /// <summary>
        /// Caster must be within this distance of the object in order to cast the associated spell
        /// </summary>
        public int Radius
        {
            get { return this.Fields[1]; }
        }

        /// <summary>
        /// TOOD: find out what this means and possibly change its type to bool or enum or whatever.
        ///  </summary>
        public int ServerOnly
        {
            get { return this.Fields[3]; }
        }

        /// <summary>The Id of the quest required to be active</summary>
        public override uint QuestId
        {
            get { return (uint) this.Fields[4]; }
        }

        public bool Large
        {
            get { return this.Fields[5] != 0; }
        }

        public int FloatingTooltip
        {
            get { return this.Fields[6]; }
        }

        protected internal override void InitEntry()
        {
            this.LinkedTrapId = (uint) this.Fields[2];
        }
    }
}