using System;
using WCell.Constants.Talents;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Talents
{
    [Serializable]
    public class TalentEntry
    {
        public TalentId Id;
        public TalentTree Tree;
        public uint Row;
        public uint Col;

        /// <summary>Spells indexed by Talent-Rank</summary>
        public Spell[] Spells;

        /// <summary>
        /// The id of the talent that must be learnt before this one can be learnt
        /// </summary>
        public TalentId RequiredId;

        /// <summary>The required rank of the required talent</summary>
        public uint RequiredRank;

        public uint Index;

        /// <summary>
        /// Required amount of points spent within the same TalentTree to activate this talent
        /// </summary>
        public uint RequiredTreePoints
        {
            get { return this.Row * 5U; }
        }

        /// <summary>The highest rank that this talent has</summary>
        public int MaxRank
        {
            get { return this.Spells.Length; }
        }

        public string Name
        {
            get { return this.Spells[0].Name; }
        }

        /// <summary>
        /// Complete name of this talent (includes the FullName of the tree)
        /// </summary>
        public string FullName
        {
            get { return this.Tree.FullName + " " + this.Name; }
        }

        public override string ToString()
        {
            return this.FullName + " (Id: " + (object) (int) this.Id + ", Ranks: " + (object) this.Spells.Length +
                   ", Required: " + (object) this.RequiredId + ")";
        }
    }
}