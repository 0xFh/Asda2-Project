using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Misc
{
    public class AuraAction : IUnitAction
    {
        public Unit Attacker { get; set; }

        public Unit Victim { get; set; }

        public bool IsCritical
        {
            get { return false; }
        }

        public Aura Aura { get; set; }

        public Spell Spell
        {
            get { return this.Aura.Spell; }
        }

        /// <summary>Does nothing</summary>
        public int ReferenceCount
        {
            get { return 0; }
            set { }
        }
    }
}