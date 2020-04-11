using System;
using System.Collections.Generic;
using WCell.Constants.Skills;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
  /// <summary>
  /// Represents the requirements that are needed to advance a Skill to the next tier
  /// </summary>
  [Serializable]
  public struct SkillTiers
  {
    public uint Id;

    /// <summary>The cost of each tier</summary>
    public uint[] Costs;

    /// <summary>The limit of each tier</summary>
    public uint[] MaxValues;

    public uint GetCost(SkillTierId id)
    {
      return Costs[(uint) id];
    }

    public uint GetMaxValue(SkillTierId id)
    {
      return MaxValues[(uint) id];
    }

    public override string ToString()
    {
      if(Id <= 0U)
        return "0";
      return "Id: " + Id + ", MaxValues: " +
             MaxValues.ToString(", ");
    }

    public static bool operator ==(SkillTiers tiers, SkillTiers tier2)
    {
      return tiers.Equals(tier2);
    }

    public static bool operator !=(SkillTiers tiers, SkillTiers tier2)
    {
      return !(tiers == tier2);
    }

    public override bool Equals(object obj)
    {
      if(!(obj is SkillTiers))
        return false;
      if(MaxValues != null)
        return MaxValues.Equals(((SkillTiers) obj).MaxValues);
      return true;
    }

    public override int GetHashCode()
    {
      if(MaxValues == null)
        return 1;
      return MaxValues.GetHashCode();
    }
  }
}