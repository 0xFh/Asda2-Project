using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Skills
{
  [Serializable]
  public class SkillLine
  {
    public List<SkillAbility> InitialAbilities = new List<SkillAbility>(5);

    /// <summary>
    /// The Spells that give the different tiers of this Skill
    /// </summary>
    public List<Spell> TeachingSpells = new List<Spell>(1);

    public SkillId Id;
    public SkillCategory Category;
    public int SkillCostsDataId;

    /// <summary>The Skill's "challenge levels"</summary>
    public SkillTiers Tiers;

    /// <summary>The name of this Skill</summary>
    public string Name;

    /// <summary>The language that this Skill represents (if any).</summary>
    public ChatLanguage Language;

    /// <summary>
    /// 1 for professions, else 0 - needed by packets
    /// Also allows the skill to be unlearned
    /// </summary>
    public ushort Abandonable;

    /// <summary>
    /// The initial value of this skill, when it has just been learnt
    /// </summary>
    public uint InitialValue
    {
      get
      {
        if(Tiers.MaxValues != null && Tiers.MaxValues.Length == 1)
          return Tiers.MaxValues[0];
        return 1;
      }
    }

    /// <summary>
    /// The max-value of the skill when it has just been learnt
    /// </summary>
    public uint InitialLimit
    {
      get
      {
        if(Tiers.MaxValues == null)
          return 1;
        return Tiers.MaxValues[0];
      }
    }

    /// <summary>The highest available value for this skill.</summary>
    public uint MaxValue
    {
      get
      {
        if(Tiers.MaxValues != null)
          return Math.Max(1U, Tiers.MaxValues[Tiers.MaxValues.Length - 1]);
        return Category == SkillCategory.WeaponProficiency ? 400U : 1U;
      }
    }

    public bool HasTier(SkillTierId tier)
    {
      if(Tiers.MaxValues != null)
        return (int) tier < Tiers.MaxValues.Length;
      return false;
    }

    public SkillTierId GetTierForLevel(int value)
    {
      if(Tiers.MaxValues != null)
      {
        for(int index = 0; index < Tiers.MaxValues.Length; ++index)
        {
          uint maxValue = Tiers.MaxValues[index];
          if(value < maxValue)
            return (SkillTierId) index;
        }
      }

      return SkillTierId.End;
    }

    public Spell GetSpellForLevel(int skillLevel)
    {
      return GetSpellForTier(GetTierForLevel(skillLevel));
    }

    public Spell GetSpellForTier(SkillTierId tier)
    {
      return TeachingSpells.FirstOrDefault(spell =>
        (SkillTierId) spell.GetEffect(SpellEffectType.Skill).BasePoints == tier);
    }

    public override string ToString()
    {
      return Name + " (" + Id + ", " + Category + ", Tier: " +
             Tiers + ")";
    }
  }
}