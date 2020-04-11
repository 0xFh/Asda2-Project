using WCell.Constants;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>Defines the basics of the Paladin class.</summary>
    public class SpearClass : BaseClass
    {
        public override ClassId Id
        {
            get { return ClassId.Spear; }
        }

        public override Asda2ClassMask ClassMask
        {
            get { return Asda2ClassMask.Spear; }
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

        public override float CalculateMagicCritChance(int level, int intellect)
        {
            return (float) ((double) intellect / 80.0 + 3.33599996566772);
        }
    }
}