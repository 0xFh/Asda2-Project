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
            this.m_owner = chr;
        }

        public bool CanDualWield
        {
            get { return this.Contains(SkillId.DualWield); }
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
            foreach (Skill skill in this.m_skills.Values)
            {
                if (skill.SkillLine.Category == SkillCategory.WeaponProficiency ||
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
            if (!this.AvailableSkills.TryGetValue(id, out skillRaceClassInfo) ||
                (long) this.m_owner.Level < (long) skillRaceClassInfo.MinimumLevel)
                return (SkillLine) null;
            Skill skill;
            if ((tier == SkillTierId.Apprentice ||
                 (long) skillRaceClassInfo.SkillLine.Tiers.MaxValues.Length >= (long) tier) &&
                (this.m_skills.TryGetValue(id, out skill) && skill.CanLearnTier(tier)))
                return (SkillLine) null;
            return skillRaceClassInfo.SkillLine;
        }

        /// <summary>
        /// Tries to learn the given tier for the given skill (if allowed)
        /// </summary>
        /// <returns>Whether it succeeded</returns>
        public bool TryLearn(SkillId id)
        {
            return this.TryLearn(id, SkillTierId.Apprentice);
        }

        /// <summary>
        /// Tries to learn the given tier for the given skill (if allowed)
        /// </summary>
        /// <returns>Whether it succeeded</returns>
        public bool TryLearn(SkillId id, SkillTierId tier)
        {
            Skill skill;
            if (!this.m_skills.TryGetValue(id, out skill))
            {
                SkillRaceClassInfo skillRaceClassInfo;
                if (!this.AvailableSkills.TryGetValue(id, out skillRaceClassInfo) ||
                    (long) this.m_owner.Level < (long) skillRaceClassInfo.MinimumLevel)
                    return false;
                skill = this.Add(skillRaceClassInfo.SkillLine, false);
            }

            if (skill.CanLearnTier(tier))
            {
                skill.MaxValue = (ushort) skill.SkillLine.Tiers.GetMaxValue(tier);
                if (id == SkillId.Riding)
                    skill.CurrentValue = skill.MaxValue;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the given skill is known to the player
        /// </summary>
        public bool Contains(SkillId skill)
        {
            return this.m_skills.ContainsKey(skill);
        }

        /// <summary>
        /// Returns whether the owner has the given amount of the given skill
        /// </summary>
        public bool CheckSkill(SkillId skillId, int amount)
        {
            if (skillId == SkillId.None)
                return true;
            Skill skill = this[skillId];
            return skill != null && (amount <= 0 || (long) skill.ActualValue >= (long) amount);
        }

        /// <summary>How many professions this character can learn</summary>
        public uint FreeProfessions
        {
            get { return this.m_owner.GetUInt32(PlayerFields.CHARACTER_POINTS2); }
            set { this.m_owner.SetUInt32((UpdateFieldId) PlayerFields.CHARACTER_POINTS2, value); }
        }

        public Character Owner
        {
            get { return this.m_owner; }
        }

        public int Count
        {
            get { return this.m_skills.Count; }
        }

        /// <summary>Sets or overrides an existing skill</summary>
        public Skill this[SkillId key]
        {
            get
            {
                Skill skill;
                this.m_skills.TryGetValue(key, out skill);
                return skill;
            }
        }

        /// <summary>
        /// All skills that are available to the owner, restricted by Race/Class.
        /// </summary>
        public Dictionary<SkillId, SkillRaceClassInfo> AvailableSkills
        {
            get { return SkillHandler.RaceClassInfos[(int) this.m_owner.Race][(int) this.m_owner.Class]; }
        }

        /// <summary>
        /// Adds a new Skill to this SkillCollection if it is not added yet and allowed for this character (or ignoreRestrictions = true)
        /// </summary>
        /// <param name="ignoreRestrictions">Whether to ignore the race, class and level requirements of this skill</param>
        /// <returns>The existing or new skill or null</returns>
        public Skill GetOrCreate(SkillId id, bool ignoreRestrictions)
        {
            Skill skill;
            if (!this.m_skills.TryGetValue(id, out skill))
                skill = this.Add(id, ignoreRestrictions);
            return skill;
        }

        public uint GetValue(SkillId id)
        {
            Skill skill;
            if (!this.m_skills.TryGetValue(id, out skill))
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
                ? SkillHandler.ById.Get<SkillLine>((uint) id)
                : this.GetLineIfLearnable(id, SkillTierId.Apprentice);
            if (line != null)
                return this.Add(line, ignoreRestrictions);
            return (Skill) null;
        }

        /// <summary>Adds and returns the given Skill with initial values</summary>
        /// <param name="line"></param>
        public Skill Add(SkillLine line, bool ignoreRestrictions)
        {
            return this.Add(line, line.InitialValue, line.InitialLimit, ignoreRestrictions);
        }

        public Skill GetOrCreate(SkillId id, SkillTierId tier, bool ignoreRestrictions)
        {
            Skill skill = this.GetOrCreate(id, ignoreRestrictions);
            if (skill != null && skill.SkillLine.HasTier(tier))
                skill.MaxValue = (ushort) skill.SkillLine.Tiers.GetMaxValue(tier);
            return skill;
        }

        public Skill GetOrCreate(SkillId id, uint value, uint max)
        {
            Skill skill = this.GetOrCreate(id, false);
            if (skill != null)
            {
                skill.CurrentValue = (ushort) value;
                skill.MaxValue = (ushort) max;
            }

            return skill;
        }

        /// <summary>Adds and returns a skill with max values</summary>
        public void LearnMax(SkillId id)
        {
            this.LearnMax(SkillHandler.Get(id));
        }

        public void LearnMax(SkillLine skillLine)
        {
            this.GetOrCreate(skillLine.Id, skillLine.MaxValue, skillLine.MaxValue);
        }

        /// <summary>
        /// Add a new Skill to this SkillCollection if its not a profession or the character still has professions left
        /// </summary>
        public Skill Add(SkillId skill, uint value, uint max, bool ignoreRestrictions)
        {
            return this.Add(SkillHandler.Get(skill), value, max, ignoreRestrictions);
        }

        /// <summary>
        /// Add a new Skill to this SkillCollection if its not a profession or the character still has professions left (or ignoreRestrictions is true)
        /// </summary>
        public Skill Add(SkillLine skillLine, uint value, uint max, bool ignoreRestrictions)
        {
            if (!ignoreRestrictions && skillLine.Category == SkillCategory.Profession && this.FreeProfessions <= 0U)
                return (Skill) null;
            Skill skill = this.CreateNew(skillLine, value, max);
            this.Add(skill, true);
            if (skillLine.Category == SkillCategory.Profession)
                --this.FreeProfessions;
            return skill;
        }

        /// <summary>Adds the skill without any checks</summary>
        protected void Add(Skill skill, bool isNew)
        {
            this.m_skills.Add(skill.SkillLine.Id, skill);
            this.ByField.Add(skill.PlayerField, skill);
            if (skill.SkillLine.Category == SkillCategory.Language)
                this.m_owner.KnownLanguages.Add(skill.SkillLine.Language);
            if (!isNew)
                return;
            skill.Push();
        }

        /// <summary>Removes a skill from this character's SkillCollection</summary>
        public bool Remove(SkillId id)
        {
            Skill skill;
            if (!this.m_skills.TryGetValue(id, out skill))
                return false;
            this.Remove(skill);
            return true;
        }

        public void Remove(Skill skill)
        {
            this.m_skills.Remove(skill.SkillLine.Id);
            this.OnRemove(skill);
        }

        internal void OnRemove(Skill skill)
        {
            this.ByField.Remove(skill.PlayerField);
            if (skill.SkillLine.Category == SkillCategory.Profession &&
                this.FreeProfessions < SkillHandler.MaxProfessionsPerChar)
                ++this.FreeProfessions;
            this.m_owner.SetUInt32((UpdateFieldId) skill.PlayerField, 0U);
            this.m_owner.SetUInt32((UpdateFieldId) (skill.PlayerField + 1), 0U);
            this.m_owner.SetUInt32((UpdateFieldId) (skill.PlayerField + 2), 0U);
            if (SkillHandler.RemoveAbilitiesWithSkill)
                skill.RemoveAllAbilities();
            skill.Record.DeleteLater();
        }

        /// <summary>Returns a new Skill object</summary>
        protected Skill CreateNew(SkillLine skillLine, uint value, uint max)
        {
            return new Skill(this, this.FindFreeField(), skillLine, value, max);
        }

        /// <summary>Returns the next free Player's skill-field</summary>
        public PlayerFields FindFreeField()
        {
            PlayerFields field = PlayerFields.SKILL_INFO_1_1;
            while (field < PlayerFields.CHARACTER_POINTS1)
            {
                if (this.m_owner.GetUInt32(field) == 0U)
                    return field;
                field += 3;
            }

            throw new Exception("No more free skill-fields? Impossible!");
        }

        /// <summary>Removes all skills (can also be considered a "reset")</summary>
        public void Clear()
        {
            foreach (Skill skill in this.m_skills.Values)
                this.OnRemove(skill);
            this.m_skills.Clear();
        }

        /// <summary>
        /// Adds all skills that are allowed for the owner's race/class combination with max value
        /// </summary>
        /// <param name="learnAbilities"></param>
        public void LearnAll(bool learnAbilities)
        {
            this.LearnAll(this.m_owner.Race, this.m_owner.Class, learnAbilities);
        }

        /// <summary>
        /// Adds all skills of that race/class combination with max value
        /// </summary>
        /// <param name="learnAbilities">Whether to also learn all abilities, related to the given skills.</param>
        public void LearnAll(RaceId race, ClassId clss, bool learnAbilities)
        {
            foreach (SkillRaceClassInfo skillRaceClassInfo in SkillHandler.RaceClassInfos[(int) race][(int) clss].Values
            )
            {
                Skill skill = this.GetOrCreate(skillRaceClassInfo.SkillLine.Id, true);
                if (skill != null)
                {
                    skill.LearnMax();
                    if (learnAbilities)
                        skill.LearnAllAbilities();
                }
            }
        }

        public IEnumerator<Skill> GetEnumerator()
        {
            return (IEnumerator<Skill>) this.m_skills.Values.GetEnumerator();
        }

        public void Load()
        {
            uint num = 0;
            foreach (SkillRecord loadSkill in this.m_owner.Record.LoadSkills())
            {
                SkillLine skillLine = SkillHandler.ById[(int) (ushort) loadSkill.SkillId];
                if (skillLine == null)
                {
                    SkillCollection.log.Warn("Invalid Skill Id '{0}' in SkillRecord '{1}'", (object) loadSkill.SkillId,
                        (object) loadSkill.Guid);
                }
                else
                {
                    if (skillLine.Category == SkillCategory.Profession)
                        ++num;
                    if (this.m_skills.ContainsKey(skillLine.Id))
                        SkillCollection.log.Warn("Character {0} had Skill {1} more than once", (object) this.m_owner,
                            (object) skillLine);
                    else
                        this.Add(new Skill(this, this.FindFreeField(), loadSkill, skillLine), false);
                }
            }

            this.FreeProfessions = Math.Max(SkillHandler.MaxProfessionsPerChar - num, 0U);
        }
    }
}