using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Talents;

namespace WCell.RealmServer.Talents
{
    [Serializable]
    public class TalentTree
    {
        /// <summary>All talents of this tree</summary>
        public readonly List<TalentEntry> Talents = new List<TalentEntry>(30);

        public TalentEntry[][] TalentTable = new TalentEntry[20][];
        public TalentTreeId Id;
        public string Name;
        public ClassId Class;
        public uint TabIndex;

        /// <summary>For Pet Talents</summary>
        public uint PetTabIndex;

        /// <summary>Total amount of Talent ranks in this Tree</summary>
        public int TotalRankCount;

        /// <summary>
        /// Full name of this tree (includes the name of the Class this Tree belongs to)
        /// </summary>
        public string FullName
        {
            get { return ((int) this.Class).ToString() + " " + this.Name; }
        }

        public IEnumerator<TalentEntry> GetEnumerator()
        {
            return (IEnumerator<TalentEntry>) this.Talents.GetEnumerator();
        }

        public override string ToString()
        {
            return this.FullName + " (Id: " + (object) (int) this.Id + ")";
        }
    }
}