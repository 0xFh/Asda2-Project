using System;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class AuraCasterGroup : SpellGroup
    {
        public AuraCasterGroup()
            : this(1)
        {
        }

        public AuraCasterGroup(int aurasPerCaster)
        {
            this.MaxCount = aurasPerCaster;
        }

        /// <summary>
        /// The amount of Auras from this group that one caster may
        /// apply to a single target.
        /// </summary>
        public int MaxCount { get; set; }
    }
}