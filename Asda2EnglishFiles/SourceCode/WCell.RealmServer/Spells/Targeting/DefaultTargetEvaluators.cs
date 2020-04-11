using System;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells.Targeting
{
    /// <summary>
    /// Set of methods to evaluate whether a target is "good" for an AI caster
    /// </summary>
    public static class DefaultTargetEvaluators
    {
        /// <summary>
        /// 
        /// </summary>
        public static int NearestEvaluator(SpellEffectHandler effectHandler, WorldObject target)
        {
            WorldObject casterObject = effectHandler.Cast.CasterObject;
            if (casterObject == null)
                return 0;
            return (int) ((double) casterObject.GetDistanceSq(target) * 10.0);
        }

        public static int RandomEvaluator(SpellEffectHandler effectHandler, WorldObject target)
        {
            return Utility.Random(0, int.MaxValue);
        }

        /// <summary>AI heal spells</summary>
        public static int MostWoundedEvaluator(SpellEffectHandler effectHandler, WorldObject target)
        {
            if (!(target is Unit))
                return int.MaxValue;
            Unit unit = (Unit) target;
            return -Math.Min(unit.MaxHealth - unit.Health, effectHandler.CalcDamageValue());
        }

        /// <summary>
        /// AI heal spells.
        /// This evaluator demonstrates how to losen constraints - i.e. don't go for the best choice, but go for anyone that satisfies some condition.
        /// </summary>
        public static int AnyWoundedEvaluator(SpellEffectHandler effectHandler, WorldObject target)
        {
            if (!(target is Unit))
                return int.MaxValue;
            Unit unit = (Unit) target;
            return unit.MaxHealth - unit.Health >= effectHandler.CalcDamageValue() / 2 ? -1 : 0;
        }
    }
}