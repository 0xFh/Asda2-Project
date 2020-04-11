using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;

namespace WCell.RealmServer.Spells.Targeting
{
    public static class DefaultTargetFilters
    {
        public static void IsFriendly(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            if (!effectHandler.Cast.CasterObject.MayAttack((IFactionMember) target))
                return;
            failedReason = SpellFailedReason.TargetEnemy;
        }

        public static void IsHostile(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            if (!effectHandler.Cast.CasterObject.MayAttack((IFactionMember) target))
            {
                failedReason = SpellFailedReason.TargetFriendly;
            }
            else
            {
                if (target.CanBeHarmed)
                    return;
                failedReason = SpellFailedReason.NotHere;
            }
        }

        /// <summary>Duel target type</summary>
        /// <param name="targets"></param>
        /// <param name="target"></param>
        /// <param name="failedReason"></param>
        public static void IsHostileOrHealable(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            WorldObject casterObject = effectHandler.Cast.CasterObject;
            Spell spell = effectHandler.Cast.Spell;
            bool hasHarmfulEffects = spell.HasHarmfulEffects;
            if (spell.HasHarmfulEffects == spell.HasBeneficialEffects)
                return;
            if (hasHarmfulEffects != casterObject.MayAttack((IFactionMember) target))
            {
                if (hasHarmfulEffects)
                    failedReason = SpellFailedReason.TargetFriendly;
                else
                    failedReason = SpellFailedReason.TargetEnemy;
            }
            else
            {
                if (!hasHarmfulEffects || target.CanBeHarmed)
                    return;
                failedReason = SpellFailedReason.NotHere;
            }
        }

        public static void IsAllied(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            if (effectHandler.Cast.CasterObject.IsAlliedWith((IFactionMember) target))
                return;
            failedReason = SpellFailedReason.TargetNotInParty;
        }

        public static void IsSameClass(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            Unit casterUnit = effectHandler.Cast.CasterUnit;
            if (casterUnit != null && target is Unit && casterUnit.Class == ((Unit) target).Class)
                return;
            failedReason = SpellFailedReason.BadTargets;
        }

        public static void IsSamePartyAndClass(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            DefaultTargetFilters.IsAllied(effectHandler, target, ref failedReason);
            if (failedReason != SpellFailedReason.Ok)
                return;
            DefaultTargetFilters.IsSameClass(effectHandler, target, ref failedReason);
        }

        public static void IsInFrontAndHostile(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            DefaultTargetFilters.IsInFrontOfCaster(effectHandler, target, ref failedReason,
                new TargetFilter(DefaultTargetFilters.IsHostile));
        }

        public static void IsInFrontAndFriendly(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            DefaultTargetFilters.IsInFrontOfCaster(effectHandler, target, ref failedReason,
                new TargetFilter(DefaultTargetFilters.IsFriendly));
        }

        public static void IsInFrontOfCaster(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason, TargetFilter filter)
        {
            WorldObject casterObject = effectHandler.Cast.CasterObject;
            if (casterObject.IsPlayer && !target.IsInFrontOf(casterObject))
                failedReason = SpellFailedReason.NotInfront;
            else
                filter(effectHandler, target, ref failedReason);
        }

        /// <summary>Is caster behind target?</summary>
        public static void IsCasterBehind(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            WorldObject casterObject = effectHandler.Cast.CasterObject;
            if (!casterObject.IsBehind(target))
            {
                failedReason = SpellFailedReason.NotBehind;
            }
            else
            {
                bool hasHarmfulEffects = effectHandler.Cast.Spell.HasHarmfulEffects;
                if (hasHarmfulEffects == casterObject.MayAttack((IFactionMember) target))
                    return;
                failedReason = hasHarmfulEffects ? SpellFailedReason.TargetFriendly : SpellFailedReason.TargetEnemy;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void IsVisible(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            failedReason = effectHandler.Cast.CasterUnit.CanSee(target)
                ? SpellFailedReason.Ok
                : SpellFailedReason.BmOrInvisgod;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void IsPlayer(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            failedReason = target is Character ? SpellFailedReason.Ok : SpellFailedReason.TargetNotPlayer;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void IsNotPlayer(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            failedReason = target is Character ? SpellFailedReason.TargetIsPlayer : SpellFailedReason.Ok;
        }

        /// <summary>
        /// Only select targets that have at least half of what the effect can heal
        /// </summary>
        public static void IsWoundedEnough(SpellEffectHandler effectHandler, WorldObject target,
            ref SpellFailedReason failedReason)
        {
            if (target is Unit)
            {
                Unit unit = (Unit) target;
                if (unit.MaxHealth - unit.Health >= effectHandler.CalcDamageValue() / 2)
                    return;
            }

            failedReason = SpellFailedReason.AlreadyAtFullHealth;
        }
    }
}