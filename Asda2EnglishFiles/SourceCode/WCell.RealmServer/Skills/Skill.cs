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
            this.PlayerField = field;
            this.m_skills = skills;
            this.m_record = record;
            this.SkillLine = skillLine;
            this.m_skills.Owner.SetUInt16Low((UpdateFieldId) field, (ushort) skillLine.Id);
            this.m_skills.Owner.SetUInt16High((UpdateFieldId) field, skillLine.Abandonable);
            this.SetCurrentValueSilently(record.CurrentValue);
            this.MaxValue = record.MaxValue;
        }

        public Skill(SkillCollection skills, PlayerFields field, SkillLine skill, uint value, uint max)
        {
            this.m_record = new SkillRecord()
            {
                SkillId = skill.Id,
                OwnerId = skills.Owner.Record.Guid
            };
            this.m_skills = skills;
            this.PlayerField = field;
            this.SkillLine = skill;
            this.m_skills.Owner.SetUInt16Low((UpdateFieldId) field, (ushort) skill.Id);
            this.m_skills.Owner.SetUInt16High((UpdateFieldId) field, skill.Abandonable);
            this.CurrentValue = (ushort) value;
            this.MaxValue = (ushort) max;
            this.m_record.CreateLater();
        }

        /// <summary>The current value of this skill</summary>
        public ushort CurrentValue
        {
            get { return this.m_record.CurrentValue; }
            set
            {
                this.SetCurrentValueSilently(value);
                this.m_skills.Owner.Achievements.CheckPossibleAchievementUpdates(
                    AchievementCriteriaType.ReachSkillLevel, (uint) this.m_record.SkillId,
                    (uint) this.m_record.CurrentValue, (Unit) null);
            }
        }

        protected void SetCurrentValueSilently(ushort value)
        {
            this.m_skills.Owner.SetUInt16Low((UpdateFieldId) (this.PlayerField + 1), value);
            this.m_record.CurrentValue = value;
            if (this.SkillLine.Id != SkillId.Defense)
                return;
            this.m_skills.Owner.UpdateDefense();
        }

        /// <summary>
        /// The maximum possible value of this skill not including modifiers
        /// </summary>
        public ushort MaxValue
        {
            get { return this.m_record.MaxValue; }
            set
            {
                this.m_skills.Owner.SetUInt16High((UpdateFieldId) (this.PlayerField + 1), value);
                this.m_record.MaxValue = value;
            }
        }

        /// <summary>Returns CurrentValue + Modifier</summary>
        public uint ActualValue
        {
            get { return (uint) this.CurrentValue + (uint) this.Modifier; }
        }

        /// <summary>
        /// Either the original max of this skill or the owner's level * 5, whatever comes first
        /// </summary>
        public int ActualMax
        {
            get { return Math.Min((int) this.MaxValue, this.m_skills.Owner.Level * 5); }
        }

        /// <summary>
        /// The modifier to this skill
        /// Will be red if negative, green if positive
        /// </summary>
        public short Modifier
        {
            get { return this.m_skills.Owner.GetInt16Low((UpdateFieldId) (this.PlayerField + 2)); }
            set
            {
                this.m_skills.Owner.SetInt16Low((UpdateFieldId) (this.PlayerField + 2), value);
                if (this.SkillLine.Id != SkillId.Defense)
                    return;
                this.m_skills.Owner.UpdateDefense();
            }
        }

        /// <summary>Apparently a flat skill-bonus without colored text</summary>
        public short ModifierValue
        {
            get { return this.m_skills.Owner.GetInt16High((UpdateFieldId) (this.PlayerField + 2)); }
            set { this.m_skills.Owner.SetInt16High((UpdateFieldId) (this.PlayerField + 2), value); }
        }

        /// <summary>
        /// The persistant record that can be saved to/loaded from DB
        /// </summary>
        internal SkillRecord Record
        {
            get { return this.m_record; }
        }

        public SkillTierId CurrentTier
        {
            get
            {
                if (this.CurrentTierSpell != null)
                    return this.CurrentTierSpell.SkillTier;
                return this.SkillLine.GetTierForLevel((int) this.CurrentValue);
            }
        }

        /// <summary>The spell that represents the current tier</summary>
        public Spell CurrentTierSpell
        {
            get { return this._currentTierSpell; }
            internal set
            {
                this._currentTierSpell = value;
                this.m_skills.m_owner.Achievements.CheckPossibleAchievementUpdates(
                    AchievementCriteriaType.LearnSkillLevel, (uint) value.Ability.Skill.Id, (uint) value.SkillTier,
                    (Unit) null);
            }
        }

        /// <summary>Checks whether the given tier can be learned</summary>
        public bool CanLearnTier(SkillTierId tier)
        {
            return this.SkillLine.HasTier(tier) &&
                   (int) this.CurrentValue >= (int) this.SkillLine.Tiers.GetMaxValue(tier) - 100;
        }

        /// <summary>
        /// Gains up to maxGain skill points with the given chance.
        /// </summary>
        public void GainRand(int chance, int maxGain)
        {
            int val2 = (int) this.MaxValue - (int) this.CurrentValue;
            if (val2 <= 0)
                return;
            maxGain = Math.Min(maxGain, val2);
            int num = Utility.Random(0, 100);
            if (chance <= num)
                return;
            this.CurrentValue += (ushort) (int) Math.Ceiling((double) maxGain / 100.0 * (double) (100 - num));
        }

        /// <summary>Gains max value of this skill.</summary>
        public void LearnMax()
        {
            this.MaxValue = (ushort) this.SkillLine.MaxValue;
            this.CurrentValue = (ushort) this.SkillLine.MaxValue;
        }

        /// <summary>The player learns all abilities of this skill.</summary>
        public void LearnAllAbilities()
        {
            foreach (SkillAbility ability in SkillHandler.GetAbilities(this.SkillLine.Id))
            {
                if (ability != null)
                    this.m_skills.Owner.Spells.AddSpell(ability.Spell);
            }
        }

        /// <summary>The player unlearns all abilities of this skill.</summary>
        public void RemoveAllAbilities()
        {
            foreach (SkillAbility ability in SkillHandler.GetAbilities(this.SkillLine.Id))
            {
                if (ability != null)
                    this.m_skills.Owner.Spells.Remove(ability.Spell);
            }
        }

        /// <summary>Saves all recent changes to this Skill to the DB</summary>
        public void Save()
        {
            this.m_record.SaveAndFlush();
        }

        /// <summary>Sends this skill instantly to the owner</summary>
        public void Push()
        {
        }
    }
}