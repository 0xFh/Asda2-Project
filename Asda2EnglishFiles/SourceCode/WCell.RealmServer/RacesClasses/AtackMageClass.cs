using WCell.Constants;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>Defines the basics of the Shaman class.</summary>
    public class AtackMageClass : BaseClass
    {
        public override ClassId Id
        {
            get { return ClassId.AtackMage; }
        }

        public override Asda2ClassMask ClassMask
        {
            get { return Asda2ClassMask.Mage; }
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

        public override float CalculateMagicCritChance(int level, int intellect)
        {
            return (float) ((double) intellect / 80.0 + 2.20000004768372);
        }
    }
}