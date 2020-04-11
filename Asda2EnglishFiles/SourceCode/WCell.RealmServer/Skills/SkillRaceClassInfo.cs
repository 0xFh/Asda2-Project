using WCell.Constants;

namespace WCell.RealmServer.Skills
{
    /// <summary>
    /// Race/Class-restrictions, the Tier and initial amount of a skill.
    /// </summary>
    public class SkillRaceClassInfo
    {
        public uint Id;
        public SkillLine SkillLine;
        public RaceMask RaceMask;
        public ClassMask ClassMask;
        public SkillRaceClassFlags Flags;
        public uint MinimumLevel;
        public SkillTiers Tiers;

        /// <summary>SkillCostsData.dbc</summary>
        public uint SkillCostIndex;

        public override string ToString()
        {
            return this.ToString("");
        }

        public string ToString(string indent)
        {
            return indent + "Id: " + (object) this.Id + "\n" + indent + "Skill: " + (object) this.SkillLine + "\n" +
                   indent + "Races: " + (object) this.RaceMask + "\n" + indent + "Classes: " + (object) this.ClassMask +
                   "\n" + indent + "Flags: " + (object) this.Flags + "\n" + indent + "MinLevel: " +
                   (object) this.MinimumLevel + "\n" + indent + "Tier: " + (object) this.Tiers + "\n" + indent +
                   "SkillCostsIndex: " + (object) this.SkillCostIndex + "\n";
        }
    }
}