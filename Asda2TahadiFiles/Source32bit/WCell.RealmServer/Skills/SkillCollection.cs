using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.Util;

namespace WCell.RealmServer.Skills
{
  /// <summary>
  /// A collection of all of one <see cref="T:WCell.RealmServer.Entities.Character" />'s skills.
  /// </summary>
  public class SkillCollection
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<SkillId, Skill> m_skills = new Dictionary<SkillId, Skill>(40);
    private readonly Dictionary<PlayerFields, Skill> ByField = new Dictionary<PlayerFields, Skill>();
    internal Character m_owner;

    public SkillCollection(Character chr)
    {
      m_owner = chr;
    }

    public bool CanDualWield
    {
      get { return Contains(SkillId.DualWield); }
    }

    public ItemSubClassMask WeaponProficiency { get; set; }

    public ItemSubClassMask ArmorProficiency { get; set; }

    public void GainWeaponSkill(int targetLevel, IAsda2Weapon weapon)
    {
    }

    public void GainDefenseSkill(int attackerLevel)
    {
    }

    internal void UpdateSkillsForLevel(int level)
    {
      foreach(Skill skill in m_skills.Values)
      {
        if(skill.SkillLine.Category == SkillCategory.WeaponProficiency ||
           skill.SkillLine.Category == SkillCategory.ArmorProficiency)
          skill.MaxValue = (ushort) (5 * level);
      }
    }

    /// <summary>
    /// If this char is allowed to learn this skill (matching Race, Class and Level) on the given tier,
    /// the correspdonding SkillLine will be returned. Returns null if skill cannot be learnt.
    /// </summary>
    public SkillLine GetLineIfLearnable(SkillId id, SkillTierId tier)
    {
      SkillRaceClassInfo skillRaceClassInfo;
      if(!AvailableSkills.TryGetValue(id, out skillRaceClassInfo) ||
         m_owner.Level < skillRaceClassInfo.MinimumLevel)
        return null;
      Skill skill;
      if((tier == SkillTierId.Apprentice ||
          skillRaceClassInfo.SkillLine.Tiers.MaxValues.Length >= (long) tier) &&
         (m_skills.TryGetValue(id, out skill) && skill.CanLearnTier(tier)))
        return null;
      return skillRaceClassInfo.SkillLine;
    }

    /// <summary>
    /// Tries to learn the given tier for the given skill (if allowed)
    /// </summary>
    /// <returns>Whether it succeeded</returns>
    public bool TryLearn(SkillId id)
    {
      return TryLearn(id, SkillTierId.Apprentice);
    }

    /// <summary>
    /// Tries to learn the given tier for the given skill (if allowed)
    /// </summary>
    /// <returns>Whether it succeeded</returns>
    public bool TryLearn(SkillId id, SkillTierId tier)
    {
      Skill skill;
      if(!m_skills.TryGetValue(id, out skill))
      {
        SkillRaceClassInfo skillRaceClassInfo;
        if(!AvailableSkills.TryGetValue(id, out skillRaceClassInfo) ||
           m_owner.Level < skillRaceClassInfo.MinimumLevel)
          return false;
        skill = Add(skillRaceClassInfo.SkillLine, false);
      }

      if(skill.CanLearnTier(tier))
      {
        skill.MaxValue = (ushort) skill.SkillLine.Tiers.GetMaxValue(tier);
        if(id == SkillId.Riding)
          skill.CurrentValue = skill.MaxValue;
      }

      return true;
    }

    /// <summary>
    /// Returns whether the given skill is known to the player
    /// </summary>
    public bool Contains(SkillId skill)
    {
      return m_skills.ContainsKey(skill);
    }

    /// <summary>
    /// Returns whether the owner has the given amount of the given skill
    /// </summary>
    public bool CheckSkill(SkillId skillId, int amount)
    {
      if(skillId == SkillId.None)
        return true;
      Skill skill = this[skillId];
      return skill != null && (amount <= 0 || skill.ActualValue >= amount);
    }

    /// <summary>How many professions this character can learn</summary>
    public uint FreeProfessions
    {
      get { return m_owner.GetUInt32(PlayerFields.CHARACTER_POINTS2); }
      set { m_owner.SetUInt32(PlayerFields.CHARACTER_POINTS2, value); }
    }

    public Character Owner
    {
      get { return m_owner; }
    }

    public int Count
    {
      get { return m_skills.Count; }
    }

    /// <summary>Sets or overrides an existing skill</summary>
    public Skill this[SkillId key]
    {
      get
      {
        Skill skill;
        m_skills.TryGetValue(key, out skill);
        return skill;
      }
    }

    /// <summary>
    /// All skills that are available to the owner, restricted by Race/Class.
    /// </summary>
    public Dictionary<SkillId, SkillRaceClassInfo> AvailableSkills
    {
      get { return SkillHandler.RaceClassInfos[(int) m_owner.Race][(int) m_owner.Class]; }
    }

    /// <summary>
    /// Adds a new Skill to this SkillCollection if it is not added yet and allowed for this character (or ignoreRestrictions = true)
    /// </summary>
    /// <param name="ignoreRestrictions">Whether to ignore the race, class and level requirements of this skill</param>
    /// <returns>The existing or new skill or null</returns>
    public Skill GetOrCreate(SkillId id, bool ignoreRestrictions)
    {
      Skill skill;
      if(!m_skills.TryGetValue(id, out skill))
        skill = Add(id, ignoreRestrictions);
      return skill;
    }

    public uint GetValue(SkillId id)
    {
      Skill skill;
      if(!m_skills.TryGetValue(id, out skill))
        return 0;
      return skill.ActualValue;
    }

    /// <summary>
    /// Add a new Skill with initial values to this SkillCollection if it can be added
    /// </summary>
    /// <param name="ignoreRestrictions">Whether to ignore the race, class and level requirements of this skill</param>
    public Skill Add(SkillId id, bool ignoreRestrictions)
    {
      SkillLine line = ignoreRestrictions
        ? SkillHandler.ById.Get((uint) id)
        : GetLineIfLearnable(id, SkillTierId.Apprentice);
      if(line != null)
        return Add(line, ignoreRestrictions);
      return null;
    }

    /// <summary>Adds and returns the given Skill with initial values</summary>
    /// <param name="line"></param>
    public Skill Add(SkillLine line, bool ignoreRestrictions)
    {
      return Add(line, line.InitialValue, line.InitialLimit, ignoreRestrictions);
    }

    public Skill GetOrCreate(SkillId id, SkillTierId tier, bool ignoreRestrictions)
    {
      Skill skill = GetOrCreate(id, ignoreRestrictions);
      if(skill != null && skill.SkillLine.HasTier(tier))
        skill.MaxValue = (ushort) skill.SkillLine.Tiers.GetMaxValue(tier);
      return skill;
    }

    public Skill GetOrCreate(SkillId id, uint value, uint max)
    {
      Skill skill = GetOrCreate(id, false);
      if(skill != null)
      {
        skill.CurrentValue = (ushort) value;
        skill.MaxValue = (ushort) max;
      }

      return skill;
    }

    /// <summary>Adds and returns a skill with max values</summary>
    public void LearnMax(SkillId id)
    {
      LearnMax(SkillHandler.Get(id));
    }

    public void LearnMax(SkillLine skillLine)
    {
      GetOrCreate(skillLine.Id, skillLine.MaxValue, skillLine.MaxValue);
    }

    /// <summary>
    /// Add a new Skill to this SkillCollection if its not a profession or the character still has professions left
    /// </summary>
    public Skill Add(SkillId skill, uint value, uint max, bool ignoreRestrictions)
    {
      return Add(SkillHandler.Get(skill), value, max, ignoreRestrictions);
    }

    /// <summary>
    /// Add a new Skill to this SkillCollection if its not a profession or the character still has professions left (or ignoreRestrictions is true)
    /// </summary>
    public Skill Add(SkillLine skillLine, uint value, uint max, bool ignoreRestrictions)
    {
      if(!ignoreRestrictions && skillLine.Category == SkillCategory.Profession && FreeProfessions <= 0U)
        return null;
      Skill skill = CreateNew(skillLine, value, max);
      Add(skill, true);
      if(skillLine.Category == SkillCategory.Profession)
        --FreeProfessions;
      return skill;
    }

    /// <summary>Adds the skill without any checks</summary>
    protected void Add(Skill skill, bool isNew)
    {
      m_skills.Add(skill.SkillLine.Id, skill);
      ByField.Add(skill.PlayerField, skill);
      if(skill.SkillLine.Category == SkillCategory.Language)
        m_owner.KnownLanguages.Add(skill.SkillLine.Language);
      if(!isNew)
        return;
      skill.Push();
    }

    /// <summary>Removes a skill from this character's SkillCollection</summary>
    public bool Remove(SkillId id)
    {
      Skill skill;
      if(!m_skills.TryGetValue(id, out skill))
        return false;
      Remove(skill);
      return true;
    }

    public void Remove(Skill skill)
    {
      m_skills.Remove(skill.SkillLine.Id);
      OnRemove(skill);
    }

    internal void OnRemove(Skill skill)
    {
      ByField.Remove(skill.PlayerField);
      if(skill.SkillLine.Category == SkillCategory.Profession &&
         FreeProfessions < SkillHandler.MaxProfessionsPerChar)
        ++FreeProfessions;
      m_owner.SetUInt32(skill.PlayerField, 0U);
      m_owner.SetUInt32(skill.PlayerField + 1, 0U);
      m_owner.SetUInt32(skill.PlayerField + 2, 0U);
      if(SkillHandler.RemoveAbilitiesWithSkill)
        skill.RemoveAllAbilities();
      skill.Record.DeleteLater();
    }

    /// <summary>Returns a new Skill object</summary>
    protected Skill CreateNew(SkillLine skillLine, uint value, uint max)
    {
      return new Skill(this, FindFreeField(), skillLine, value, max);
    }

    /// <summary>Returns the next free Player's skill-field</summary>
    public PlayerFields FindFreeField()
    {
      PlayerFields field = PlayerFields.SKILL_INFO_1_1;
      while(field < PlayerFields.CHARACTER_POINTS1)
      {
        if(m_owner.GetUInt32(field) == 0U)
          return field;
        field += 3;
      }

      throw new Exception("No more free skill-fields? Impossible!");
    }

    /// <summary>Removes all skills (can also be considered a "reset")</summary>
    public void Clear()
    {
      foreach(Skill skill in m_skills.Values)
        OnRemove(skill);
      m_skills.Clear();
    }

    /// <summary>
    /// Adds all skills that are allowed for the owner's race/class combination with max value
    /// </summary>
    /// <param name="learnAbilities"></param>
    public void LearnAll(bool learnAbilities)
    {
      LearnAll(m_owner.Race, m_owner.Class, learnAbilities);
    }

    /// <summary>
    /// Adds all skills of that race/class combination with max value
    /// </summary>
    /// <param name="learnAbilities">Whether to also learn all abilities, related to the given skills.</param>
    public void LearnAll(RaceId race, ClassId clss, bool learnAbilities)
    {
      foreach(SkillRaceClassInfo skillRaceClassInfo in SkillHandler.RaceClassInfos[(int) race][(int) clss].Values
      )
      {
        Skill skill = GetOrCreate(skillRaceClassInfo.SkillLine.Id, true);
        if(skill != null)
        {
          skill.LearnMax();
          if(learnAbilities)
            skill.LearnAllAbilities();
        }
      }
    }

    public IEnumerator<Skill> GetEnumerator()
    {
      return m_skills.Values.GetEnumerator();
    }

    public void Load()
    {
      uint num = 0;
      foreach(SkillRecord loadSkill in m_owner.Record.LoadSkills())
      {
        SkillLine skillLine = SkillHandler.ById[(ushort) loadSkill.SkillId];
        if(skillLine == null)
        {
          log.Warn("Invalid Skill Id '{0}' in SkillRecord '{1}'", loadSkill.SkillId,
            loadSkill.Guid);
        }
        else
        {
          if(skillLine.Category == SkillCategory.Profession)
            ++num;
          if(m_skills.ContainsKey(skillLine.Id))
            log.Warn("Character {0} had Skill {1} more than once", m_owner,
              skillLine);
          else
            Add(new Skill(this, FindFreeField(), loadSkill, skillLine), false);
        }
      }

      FreeProfessions = Math.Max(SkillHandler.MaxProfessionsPerChar - num, 0U);
    }
  }
}