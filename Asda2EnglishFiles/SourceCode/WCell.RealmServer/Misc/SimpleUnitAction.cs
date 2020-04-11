using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Misc
{
    public class SimpleUnitAction : IUnitAction
    {
        public Unit Attacker { get; set; }

        public Unit Victim { get; set; }

        public bool IsCritical { get; set; }

        public Spell Spell { get; set; }

        /// <summary>Does nothing</summary>
        public int ReferenceCount
        {
            get { return 0; }
            set { }
        }
    }
}