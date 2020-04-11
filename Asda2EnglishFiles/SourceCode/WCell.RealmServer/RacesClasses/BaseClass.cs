using System;
using WCell.Constants;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>
    /// Defines the basics of a class.
    /// NOTE that all equations don't take boni from combat ratings into account
    /// </summary>
    public abstract class BaseClass
    {
        public static int DefaultStartLevel = 1;

        /// <summary>Basic class settings per level</summary>
        public ClassLevelSetting[] Settings = new ClassLevelSetting[RealmServerConfiguration.MaxCharacterLevel];

        public int BaseHealth;
        public int BasePower;

        /// <summary>All SpellLines of this class</summary>
        public SpellLine[] SpellLines;

        /// <summary>The class type this class represents.</summary>
        public abstract ClassId Id { get; }

        public int ActualStartLevel
        {
            get { return Math.Max(this.StartLevel, BaseClass.DefaultStartLevel); }
        }

        public virtual int StartLevel
        {
            get { return 1; }
        }

        /// <summary>The PowerType this class uses.</summary>
        public virtual PowerType DefaultPowerType
        {
            get { return PowerType.Mana; }
        }

        public abstract Asda2ClassMask ClassMask { get; }

        public ClassLevelSetting GetLevelSetting(int level)
        {
            if (level >= this.Settings.Length)
                level = this.Settings.Length - 1;
            else if (level < 1)
                level = 1;
            ClassLevelSetting setting = this.Settings[level];
            if (setting != null)
                return setting;
            ContentMgr.OnInvalidDBData("{0} has no ClassLevelSetting for level {1}", (object) this, (object) level);
            return new ClassLevelSetting();
        }

        /// <summary>
        /// Calculates attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total attack power</returns>
        public abstract int CalculateMeleeAP(int level, int strength, int agility);

        /// <summary>
        /// Calculates the dodge amount for the class at a specific level and Agility.
        /// TODO: Find the constant that can be used to get 1 agi = X % dodge (diminishing returns)
        /// http://wowwiki.com/Dodge
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total dodge amount</returns>
        public virtual float CalculateDodge(int level, int agility, int baseAgility, int defenseSkill, int dodgeRating,
            int defenseRating)
        {
            return 5f;
        }

        /// <summary>Calculates the parry chance from parry rating.</summary>
        /// <param name="level"></param>
        /// <param name="parryRating"></param>
        /// <param name="str">Strength, used for DKs</param>
        /// <returns></returns>
        public virtual float CalculateParry(int level, int parryRating, int str)
        {
            return 5f;
        }

        /// <summary>
        /// Calculates the amount of health regeneration for the class at a specific level and Spirit.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="spirit">the player's Spirit</param>
        /// <returns>the total health regeneration amount</returns>
        public int CalculateHealthRegen(Unit unit)
        {
            int num = unit.MaxHealth / 1000;
            if (num != 0)
                return num;
            return 1;
        }

        /// <summary>
        /// Calculates the magic critical chance for the class at a specific level, base Intellect and added Intellect.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total magic critical chance</returns>
        public virtual float CalculateMagicCritChance(int level, int intellect)
        {
            return 5f;
        }

        /// <summary>
        /// Calculates the melee critical chance for the class at a specific level, base Agility and added Agility.
        /// TODO: Implement diminishing returns
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total melee critical chance</returns>
        public float CalculateMeleeCritChance(int level, int agility, int luck)
        {
            float psysicCritChance = CharacterFormulas.CalculatePsysicCritChance(this.Id, level, luck);
            if ((double) psysicCritChance <= 5.0)
                return 5f;
            return psysicCritChance;
        }

        /// <summary>
        /// TODO: Ranged Crit Chance
        /// http://www.wowwiki.com/Formulas:Critical_hit_chance
        /// http://www.wowwiki.com/Formulas:Agility
        /// </summary>
        public float CalculateRangedCritChance(int level, int agility)
        {
            return 5f;
        }

        /// <summary>
        /// Calculates ranged attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total ranged attack power</returns>
        public virtual int CalculateRangedAP(int level, int strength, int agility)
        {
            return agility - 10;
        }

        /// <summary>
        /// Gets the total health gained for the class at a specific level.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total health gained up until the given level</returns>
        public int GetHealthForLevel(int level)
        {
            return this.GetLevelSetting(level).Health;
        }

        /// <summary>
        /// Runs any needed initialization for a player that has just been created.
        /// </summary>
        /// <param name="character">the <see cref="T:WCell.RealmServer.Entities.Character">Character</see> that needs to be initialized</param>
        public virtual void InitializeStartingPlayer(Character character)
        {
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        public void FinalizeAfterLoad()
        {
            ArchetypeMgr.BaseClasses[(uint) this.Id] = this;
        }
    }
}