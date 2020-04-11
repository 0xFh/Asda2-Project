using WCell.Constants;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>Defines the basics of the Rogue class.</summary>
    public class CrossbowClass : BaseClass
    {
        public override ClassId Id
        {
            get { return ClassId.Crossbow; }
        }

        public override Asda2ClassMask ClassMask
        {
            get { return Asda2ClassMask.Crossbow; }
        }

        /// <summary>
        /// Calculates attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total attack power</returns>
        public override int CalculateMeleeAP(int level, int strength, int agility)
        {
            return level * 2 + strength + agility - 20;
        }

        /// <summary>
        /// Calculates ranged attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total ranged attack power</returns>
        public override int CalculateRangedAP(int level, int strength, int agility)
        {
            return level + agility - 10;
        }

        /// <summary>
        /// Calculates the dodge amount for the class at a specific level and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total dodge amount</returns>
        public override float CalculateDodge(int level, int agility, int baseAgility, int defenseSkill, int dodgeRating,
            int defense)
        {
            return (float) agility / (float) ((double) level * 0.225999996066093 + 0.837999999523163);
        }

        public override float CalculateMagicCritChance(int level, int intellect)
        {
            return 0.0f;
        }
    }
}