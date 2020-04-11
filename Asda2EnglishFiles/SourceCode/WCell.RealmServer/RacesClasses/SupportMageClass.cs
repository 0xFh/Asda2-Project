using WCell.Constants;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>Defines the basics of the Mage class.</summary>
    public class SupportMageClass : BaseClass
    {
        public override ClassId Id
        {
            get { return ClassId.SupportMage; }
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
            return strength - 10;
        }

        public override float CalculateMagicCritChance(int level, int intellect)
        {
            return (float) ((double) intellect / 80.0 + 0.910000026226044);
        }
    }
}