using WCell.Constants;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>Defines the basics of the Warrior class.</summary>
    public class OHSClass : BaseClass
    {
        public override ClassId Id
        {
            get { return ClassId.OHS; }
        }

        public override PowerType DefaultPowerType
        {
            get { return PowerType.Mana; }
        }

        public override Asda2ClassMask ClassMask
        {
            get { return Asda2ClassMask.OHS; }
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
            return level * 3 + strength * 2 - 20;
        }

        public override int CalculateRangedAP(int level, int strength, int agility)
        {
            return level + agility - 10;
        }

        public override float CalculateMagicCritChance(int level, int intellect)
        {
            return 0.0f;
        }
    }
}