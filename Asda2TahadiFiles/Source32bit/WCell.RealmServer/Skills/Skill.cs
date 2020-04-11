using System;
using WCell.Constants.Achievements;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
  /// <summary>Represents a Player's progress with a certain skill</summary>
  public class Skill
  {
    public readonly PlayerFields PlayerField;
    public readonly SkillLine SkillLine;

    /// <summary>The containing SkillCollection</summary>
    private readonly SkillCollection m_skills;

    private readonly SkillRecord m_record;
    private Spell _currentTierSpell;

    public Skill(SkillCollection skills, PlayerFields field, SkillRecord record, SkillLine skillLine)
    {
      PlayerField = field;
      m_skills = skills;
      m_record = record;
      SkillLine = skillLine;
      m_skills.Owner.SetUInt16Low(field, (ushort) skillLine.Id);
      m_skills.Owner.SetUInt16High(field, skillLine.Abandonable);
      SetCurrentValueSilently(record.CurrentValue);
      MaxValue = record.MaxValue;
    }

    public Skill(SkillCollection skills, PlayerFields field, SkillLine skill, uint value, uint max)
    {
      m_record = new SkillRecord
      {
        SkillId = skill.Id,
        OwnerId = skills.Owner.Record.Guid
      };
      m_skills = skills;
      PlayerField = field;
      SkillLine = skill;
      m_skills.Owner.SetUInt16Low(field, (ushort) skill.Id);
      m_skills.Owner.SetUInt16High(field, skill.Abandonable);
      CurrentValue = (ushort) value;
      MaxValue = (ushort) max;
      m_record.CreateLater();
    }

    /// <summary>The current value of this skill</summary>
    public ushort CurrentValue
    {
      get { return m_record.CurrentValue; }
      set
      {
        SetCurrentValueSilently(value);
        m_skills.Owner.Achievements.CheckPossibleAchievementUpdates(
          AchievementCriteriaType.ReachSkillLevel, (uint) m_record.SkillId,
          m_record.CurrentValue, null);
      }
    }

    protected void SetCurrentValueSilently(ushort value)
    {
      m_skills.Owner.SetUInt16Low(PlayerField + 1, value);
      m_record.CurrentValue = value;
      if(SkillLine.Id != SkillId.Defense)
        return;
      m_skills.Owner.UpdateDefense();
    }

    /// <summary>
    /// The maximum possible value of this skill not including modifiers
    /// </summary>
    public ushort MaxValue
    {
      get { return m_record.MaxValue; }
      set
      {
        m_skills.Owner.SetUInt16High(PlayerField + 1, value);
        m_record.MaxValue = value;
      }
    }

    /// <summary>Returns CurrentValue + Modifier</summary>
    public uint ActualValue
    {
      get { return CurrentValue + (uint) Modifier; }
    }

    /// <summary>
    /// Either the original max of this skill or the owner's level * 5, whatever comes first
    /// </summary>
    public int ActualMax
    {
      get { return Math.Min(MaxValue, m_skills.Owner.Level * 5); }
    }

    /// <summary>
    /// The modifier to this skill
    /// Will be red if negative, green if positive
    /// </summary>
    public short Modifier
    {
      get { return m_skills.Owner.GetInt16Low(PlayerField + 2); }
      set
      {
        m_skills.Owner.SetInt16Low(PlayerField + 2, value);
        if(SkillLine.Id != SkillId.Defense)
          return;
        m_skills.Owner.UpdateDefense();
      }
    }

    /// <summary>Apparently a flat skill-bonus without colored text</summary>
    public short ModifierValue
    {
      get { return m_skills.Owner.GetInt16High(PlayerField + 2); }
      set { m_skills.Owner.SetInt16High(PlayerField + 2, value); }
    }

    /// <summary>
    /// The persistant record that can be saved to/loaded from DB
    /// </summary>
    internal SkillRecord Record
    {
      get { return m_record; }
    }

    public SkillTierId CurrentTier
    {
      get
      {
        if(CurrentTierSpell != null)
          return CurrentTierSpell.SkillTier;
        return SkillLine.GetTierForLevel(CurrentValue);
      }
    }

    /// <summary>The spell that represents the current tier</summary>
    public Spell CurrentTierSpell
    {
      get { return _currentTierSpell; }
      internal set
      {
        _currentTierSpell = value;
        m_skills.m_owner.Achievements.CheckPossibleAchievementUpdates(
          AchievementCriteriaType.LearnSkillLevel, (uint) value.Ability.Skill.Id, (uint) value.SkillTier,
          null);
      }
    }

    /// <summary>Checks whether the given tier can be learned</summary>
    public bool CanLearnTier(SkillTierId tier)
    {
      return SkillLine.HasTier(tier) &&
             CurrentValue >= (int) SkillLine.Tiers.GetMaxValue(tier) - 100;
    }

    /// <summary>
    /// Gains up to maxGain skill points with the given chance.
    /// </summary>
    public void GainRand(int chance, int maxGain)
    {
      int val2 = MaxValue - CurrentValue;
      if(val2 <= 0)
        return;
      maxGain = Math.Min(maxGain, val2);
      int num = Utility.Random(0, 100);
      if(chance <= num)
        return;
      CurrentValue += (ushort) (int) Math.Ceiling(maxGain / 100.0 * (100 - num));
    }

    /// <summary>Gains max value of this skill.</summary>
    public void LearnMax()
    {
      MaxValue = (ushort) SkillLine.MaxValue;
      CurrentValue = (ushort) SkillLine.MaxValue;
    }

    /// <summary>The player learns all abilities of this skill.</summary>
    public void LearnAllAbilities()
    {
      foreach(SkillAbility ability in SkillHandler.GetAbilities(SkillLine.Id))
      {
        if(ability != null)
          m_skills.Owner.Spells.AddSpell(ability.Spell);
      }
    }

    /// <summary>The player unlearns all abilities of this skill.</summary>
    public void RemoveAllAbilities()
    {
      foreach(SkillAbility ability in SkillHandler.GetAbilities(SkillLine.Id))
      {
        if(ability != null)
          m_skills.Owner.Spells.Remove(ability.Spell);
      }
    }

    /// <summary>Saves all recent changes to this Skill to the DB</summary>
    public void Save()
    {
      m_record.SaveAndFlush();
    }

    /// <summary>Sends this skill instantly to the owner</summary>
    public void Push()
    {
    }
  }
}