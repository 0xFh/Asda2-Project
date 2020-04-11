using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Targeting;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Spells
{
    /// <summary>A list of all targets for a Spell</summary>
    public class SpellTargetCollection : List<WorldObject>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static readonly ObjectPool<SpellTargetCollection> SpellTargetCollectionPool =
            ObjectPoolMgr.CreatePool<SpellTargetCollection>(
                (Func<SpellTargetCollection>) (() => new SpellTargetCollection()));

        /// <summary>
        /// Further SpellEffectHandlers that are sharing this TargetCollection with the original Handler
        /// </summary>
        internal List<SpellEffectHandler> m_handlers;

        public static SpellTargetCollection Obtain()
        {
            return new SpellTargetCollection();
        }

        /// <summary>Private ctor used by SpellTargetCollectionPool</summary>
        public SpellTargetCollection()
        {
            this.m_handlers = new List<SpellEffectHandler>(3);
        }

        public bool IsInitialized { get; private set; }

        public SpellEffectHandler FirstHandler
        {
            get { return this.m_handlers[0]; }
        }

        public SpellCast Cast
        {
            get { return this.m_handlers[0].Cast; }
        }

        /// <summary>
        /// Compares targets if the effect is to select a limited amount of targets from an arbitrary set.
        /// Mostly used by AI casters.
        /// For example, AI casters should select the most wounded as target for healing spells.
        /// </summary>
        public TargetEvaluator TargetEvaluator
        {
            get
            {
                SpellEffectHandler firstHandler = this.FirstHandler;
                return firstHandler.Effect.GetTargetEvaluator(firstHandler.Cast.IsAICast);
            }
        }

        public SpellFailedReason AddAll(WorldObject[] forcedTargets)
        {
            this.IsInitialized = true;
            for (int index = 0; index < forcedTargets.Length; ++index)
            {
                WorldObject forcedTarget = forcedTargets[index];
                if (forcedTarget == null)
                    LogManager.GetCurrentClassLogger()
                        .Warn("{0} tried to cast spell \"{1}\" with forced target which is null",
                            (object) this.Cast.CasterObject, (object) this.Cast.Spell);
                else if (forcedTarget.IsInContext)
                {
                    SpellFailedReason spellFailedReason = this.ValidateTargetForHandlers(forcedTarget);
                    if (spellFailedReason != SpellFailedReason.Ok)
                    {
                        LogManager.GetCurrentClassLogger().Warn(
                            "{0} tried to cast spell \"{1}\" with forced target {2} which is not valid: {3}",
                            (object) this.Cast.CasterObject, (object) this.Cast.Spell, (object) forcedTarget,
                            (object) spellFailedReason);
                        if (!this.Cast.IsAoE)
                            return spellFailedReason;
                    }
                    else
                        this.Add(forcedTarget);
                }
                else if (forcedTarget.IsInWorld)
                    LogManager.GetCurrentClassLogger()
                        .Warn("{0} tried to cast spell \"{1}\" with forced target {2} which is not in context",
                            (object) this.Cast.CasterObject, (object) this.Cast.Spell, (object) forcedTarget);
            }

            return SpellFailedReason.Ok;
        }

        private int EvaluateTarget(WorldObject target)
        {
            SpellEffectHandler firstHandler = this.FirstHandler;
            SpellCast cast = firstHandler.Cast;
            int num = this.TargetEvaluator(firstHandler, target);
            if (cast.IsAICast && cast.Spell.IsAura && (target is Unit && ((Unit) target).Auras.Contains(cast.Spell)))
                num += 1000000;
            return num;
        }

        /// <summary>
        /// Adds the given target.
        /// If an evaluator is set and the limit is reached, replace the worst existing target, if its worse than the new target.
        /// A *bigger* value returned by the evaluator is worse than a *smaller* one.
        /// </summary>
        public bool AddOrReplace(WorldObject target, int limit)
        {
            if (this.TargetEvaluator == null)
            {
                this.Add(target);
                return this.Count < limit;
            }

            if (this.Count < limit)
            {
                this.Add(target);
            }
            else
            {
                int num = this.EvaluateTarget(target);
                int index1 = -1;
                for (int index2 = this.Count - 1; index2 >= 0; --index2)
                {
                    int target1 = this.EvaluateTarget(this[index2]);
                    if (target1 > num)
                    {
                        num = target1;
                        index1 = index2;
                    }
                }

                if (index1 > -1)
                    this[index1] = target;
            }

            return true;
        }

        public void AddSingleTarget(WorldObject target)
        {
            if (this.TargetEvaluator != null)
                SpellTargetCollection.log.Warn(
                    "Target Evaluator is not null, but only a single target adder was used - Consider using an \"AddArea*\" adder to add the best choice from any possible nearby target for spell: " +
                    (object) this.FirstHandler.Effect.Spell);
            this.Add(target);
        }

        public SpellFailedReason FindAllTargets()
        {
            this.IsInitialized = true;
            WorldObject casterObject = this.Cast.CasterObject;
            if (casterObject == null)
            {
                SpellTargetCollection.log.Warn("Invalid SpellCast - Tried to find targets, without Caster set: {0}",
                    (object) casterObject);
                return SpellFailedReason.Error;
            }

            SpellEffect effect = this.FirstHandler.Effect;
            if (effect.Spell.IsPreventionDebuff)
            {
                SpellFailedReason spellFailedReason = SpellFailedReason.Ok;
                foreach (SpellEffectHandler handler in this.m_handlers)
                {
                    spellFailedReason = handler.ValidateAndInitializeTarget(casterObject);
                    if (spellFailedReason != SpellFailedReason.Ok)
                        return spellFailedReason;
                }

                this.Add(casterObject);
                return spellFailedReason;
            }

            SpellFailedReason spellFailedReason1 = SpellFailedReason.Ok;
            TargetDefinition targetDefinition = effect.GetTargetDefinition();
            if (targetDefinition != null)
            {
                SpellFailedReason failReason = SpellFailedReason.Ok;
                targetDefinition.Collect(this, ref failReason);
            }
            else
            {
                if (effect.ImplicitTargetA != ImplicitSpellTargetType.None)
                {
                    spellFailedReason1 = this.FindTargets(effect.ImplicitTargetA);
                    if (spellFailedReason1 != SpellFailedReason.Ok)
                        return spellFailedReason1;
                }

                if (effect.ImplicitTargetB != ImplicitSpellTargetType.None)
                    spellFailedReason1 = this.FindTargets(effect.ImplicitTargetB);
            }

            return spellFailedReason1;
        }

        public SpellFailedReason FindTargets(ImplicitSpellTargetType targetType)
        {
            SpellFailedReason failReason = SpellFailedReason.Ok;
            TargetDefinition targetDefinition = DefaultTargetDefinitions.GetTargetDefinition(targetType);
            if (targetDefinition != null)
                targetDefinition.Collect(this, ref failReason);
            return failReason;
        }

        /// <summary>
        /// Adds all chained units around the selected unit to the list
        /// </summary>
        public void FindChain(Unit first, TargetFilter filter, bool harmful, int limit)
        {
            SpellEffectHandler firstHandler = this.FirstHandler;
            WorldObject caster = firstHandler.Cast.CasterObject;
            Spell spell = firstHandler.Cast.Spell;
            first.IterateEnvironment(firstHandler.GetRadius(), (Func<WorldObject, bool>) (target =>
            {
                if ((spell.FacingFlags & SpellFacingFlags.RequiresInFront) != (SpellFacingFlags) 0 && caster != null &&
                    !target.IsInFrontOf(caster) || (target == caster || target == first || caster == null) ||
                    (harmful && !caster.MayAttack((IFactionMember) target) ||
                     !harmful && !caster.IsInSameDivision((IFactionMember) target) ||
                     this.ValidateTarget(target, filter) != SpellFailedReason.Ok))
                    return true;
                this.AddOrReplace(target, limit);
                return true;
            }));
            if (this.TargetEvaluator != null)
                return;
            this.Sort((Comparison<WorldObject>) ((a, b) =>
                a.GetDistanceSq((WorldObject) first).CompareTo(b.GetDistanceSq((WorldObject) first))));
            if (this.Count <= limit)
                return;
            this.RemoveRange(limit, this.Count - limit);
        }

        /// <summary>
        /// Does default checks on whether the given Target is valid for the current SpellCast
        /// </summary>
        public SpellFailedReason ValidateTarget(WorldObject target, TargetFilter filter)
        {
            SpellEffectHandler firstHandler = this.FirstHandler;
            SpellCast cast = firstHandler.Cast;
            SpellFailedReason failReason = cast.Spell.CheckValidTarget(cast.CasterObject, target);
            if (failReason != SpellFailedReason.Ok)
                return failReason;
            if (filter != null)
            {
                filter(firstHandler, target, ref failReason);
                if (failReason != SpellFailedReason.Ok)
                    return failReason;
            }

            return this.ValidateTargetForHandlers(target);
        }

        public SpellFailedReason ValidateTargetForHandlers(WorldObject target)
        {
            foreach (SpellEffectHandler handler in this.m_handlers)
            {
                if (!target.CheckObjType(handler.TargetType))
                    return SpellFailedReason.BadTargets;
                SpellFailedReason spellFailedReason = handler.ValidateAndInitializeTarget(target);
                if (spellFailedReason != SpellFailedReason.Ok)
                    return spellFailedReason;
            }

            return SpellFailedReason.Ok;
        }

        /// <summary>
        /// Removes all targets that don't satisfy the effects' constraints
        /// </summary>
        public void RevalidateAll()
        {
            SpellEffectHandler firstHandler = this.FirstHandler;
            SpellCast cast = firstHandler.Cast;
            for (int index = this.Count - 1; index >= 0; --index)
            {
                WorldObject target = this[index];
                if (target.IsInWorld)
                {
                    TargetDefinition targetDefinition = firstHandler.Effect.GetTargetDefinition();
                    if (targetDefinition != null)
                    {
                        if (this.ValidateTarget(target, targetDefinition.Filter) == SpellFailedReason.Ok)
                            continue;
                    }
                    else if (this.ValidateTarget(target,
                                 DefaultTargetDefinitions.GetTargetFilter(firstHandler.Effect.ImplicitTargetA)) ==
                             SpellFailedReason.Ok)
                    {
                        TargetFilter targetFilter =
                            DefaultTargetDefinitions.GetTargetFilter(firstHandler.Effect.ImplicitTargetB);
                        if (targetFilter != null)
                        {
                            SpellFailedReason failReason = SpellFailedReason.Ok;
                            targetFilter(firstHandler, target, ref failReason);
                            if (failReason == SpellFailedReason.Ok)
                                continue;
                        }
                        else
                            continue;
                    }
                }

                this.RemoveAt(index);
            }
        }

        public void ApplyToAllOf<TargetType>(SpellTargetCollection.TargetHandler<TargetType> handler)
            where TargetType : ObjectBase
        {
            foreach (WorldObject worldObject in (List<WorldObject>) this)
            {
                if (worldObject is TargetType)
                    handler(worldObject as TargetType);
            }
        }

        internal void Dispose()
        {
            if (!this.IsInitialized)
                return;
            this.IsInitialized = false;
            this.m_handlers.Clear();
            this.Clear();
        }

        public delegate void TargetHandler<in T>(T target);
    }
}