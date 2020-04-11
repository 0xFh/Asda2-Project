using WCell.Constants;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOAreaDamageEntry : GOEntry
    {
        /// <summary>LockId from Lock.dbc</summary>
        public int LockId
        {
            get { return this.Fields[0]; }
        }

        /// <summary>The radius within which the damage is applied (?)</summary>
        public int Radius
        {
            get { return this.Fields[1]; }
        }

        /// <summary>The minimum damage done.</summary>
        public int MinDamage
        {
            get { return this.Fields[2]; }
        }

        /// <summary>The maximum damage done.</summary>
        public int MaxDamage
        {
            get { return this.Fields[3]; }
        }

        /// <summary>The type of damage done.</summary>
        public DamageSchool DamageSchool
        {
            get { return (DamageSchool) this.Fields[4]; }
        }

        /// <summary>The duration of the damaging effect (?)</summary>
        public int AutoClose
        {
            get { return this.Fields[5]; }
        }

        /// <summary>
        /// The Id of a text object to be displayed when the AreaDamage starts. (?)
        /// </summary>
        public int OpenTextId
        {
            get { return this.Fields[6]; }
        }

        /// <summary>
        /// The Id of a text object to be displayed when the AreaDamage ends. (?)
        /// </summary>
        public int CloseTextId
        {
            get { return this.Fields[7]; }
        }

        protected internal override void InitEntry()
        {
            this.Lock = LockEntry.Entries.Get<LockEntry>((uint) this.LockId);
        }
    }
}