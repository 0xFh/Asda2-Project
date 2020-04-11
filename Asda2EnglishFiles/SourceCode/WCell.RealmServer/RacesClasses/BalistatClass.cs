using WCell.Constants;

namespace WCell.RealmServer.RacesClasses
{
    public class BalistatClass : BaseClass
    {
        public override ClassId Id
        {
            get { return ClassId.Balista; }
        }

        public override Asda2ClassMask ClassMask
        {
            get { return Asda2ClassMask.Balista; }
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
            return 0;
        }

        public override float CalculateMagicCritChance(int level, int intellect)
        {
            return (float) ((double) intellect / 80.0 + 2.20000004768372);
        }

        /// <summary>
        /// Deathknights get 25% of their str added as parry rating.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="parryRating"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public override float CalculateParry(int level, int parryRating, int str)
        {
            return base.CalculateParry(level, (int) ((double) parryRating + (double) str * 0.25), str);
        }
    }
}