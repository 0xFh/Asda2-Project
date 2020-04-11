using System;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Formulas
{
    /// <summary>
    /// <see href="http://www.wowwiki.com/Rest" />
    /// </summary>
    public static class RestGenerator
    {
        /// <summary>
        /// The amount of time in which a user can gain 5% (100 / <see cref="F:WCell.RealmServer.Formulas.RestGenerator.AverageRestingRatePct" />) rest in seconds.
        /// Default: 8 * 3600 = 8 hours.
        /// </summary>
        public static int AverageRestingPeriod = 28800;

        /// <summary>
        /// Average percentage of Rest generated per <see cref="F:WCell.RealmServer.Formulas.RestGenerator.AverageRestingPeriod" />
        /// </summary>
        public static int AverageRestingRatePct = 5;

        /// <summary>the amount of xp to the following level</summary>
        /// <remarks>By default, rest accumulates 4 times faster when in an Inn or other kind of resting area.</remarks>
        /// <returns></returns>
        public static Func<TimeSpan, Character, int> GetRestXp = (Func<TimeSpan, Character, int>) ((time, chr) =>
        {
            int nextLevelXp = chr.NextLevelXP;
            if (nextLevelXp <= 0)
                return 0;
            int totalSeconds = (int) time.TotalSeconds;
            int num1 = nextLevelXp * totalSeconds * RestGenerator.AverageRestingRatePct /
                       (100 * RestGenerator.AverageRestingPeriod);
            if (chr.RestTrigger == null)
                num1 /= 4;
            int num2 = nextLevelXp * 3 / 2;
            return MathUtil.ClampMinMax(num1, 0, num2 - chr.RestXp);
        });
    }
}