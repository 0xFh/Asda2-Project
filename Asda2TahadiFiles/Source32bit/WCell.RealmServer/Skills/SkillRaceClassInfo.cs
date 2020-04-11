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
      return ToString("");
    }

    public string ToString(string indent)
    {
      return indent + "Id: " + Id + "\n" + indent + "Skill: " + SkillLine + "\n" +
             indent + "Races: " + RaceMask + "\n" + indent + "Classes: " + ClassMask +
             "\n" + indent + "Flags: " + Flags + "\n" + indent + "MinLevel: " +
             MinimumLevel + "\n" + indent + "Tier: " + Tiers + "\n" + indent +
             "SkillCostsIndex: " + SkillCostIndex + "\n";
    }
  }
}