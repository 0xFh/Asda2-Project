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
      ObjectPoolMgr.CreatePool(
        () => new SpellTargetCollection());

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
      m_handlers = new List<SpellEffectHandler>(3);
    }

    public bool IsInitialized { get; private set; }

    public SpellEffectHandler FirstHandler
    {
      get { return m_handlers[0]; }
    }

    public SpellCast Cast
    {
      get { return m_handlers[0].Cast; }
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
        SpellEffectHandler firstHandler = FirstHandler;
        return firstHandler.Effect.GetTargetEvaluator(firstHandler.Cast.IsAICast);
      }
    }

    public SpellFailedReason AddAll(WorldObject[] forcedTargets)
    {
      IsInitialized = true;
      for(int index = 0; index < forcedTargets.Length; ++index)
      {
        WorldObject forcedTarget = forcedTargets[index];
        if(forcedTarget == null)
          LogManager.GetCurrentClassLogger()
            .Warn("{0} tried to cast spell \"{1}\" with forced target which is null",
              Cast.CasterObject, Cast.Spell);
        else if(forcedTarget.IsInContext)
        {
          SpellFailedReason spellFailedReason = ValidateTargetForHandlers(forcedTarget);
          if(spellFailedReason != SpellFailedReason.Ok)
          {
            LogManager.GetCurrentClassLogger().Warn(
              "{0} tried to cast spell \"{1}\" with forced target {2} which is not valid: {3}",
              (object) Cast.CasterObject, (object) Cast.Spell, (object) forcedTarget,
              (object) spellFailedReason);
            if(!Cast.IsAoE)
              return spellFailedReason;
          }
          else
            Add(forcedTarget);
        }
        else if(forcedTarget.IsInWorld)
          LogManager.GetCurrentClassLogger()
            .Warn("{0} tried to cast spell \"{1}\" with forced target {2} which is not in context",
              Cast.CasterObject, Cast.Spell, forcedTarget);
      }

      return SpellFailedReason.Ok;
    }

    private int EvaluateTarget(WorldObject target)
    {
      SpellEffectHandler firstHandler = FirstHandler;
      SpellCast cast = firstHandler.Cast;
      int num = TargetEvaluator(firstHandler, target);
      if(cast.IsAICast && cast.Spell.IsAura && (target is Unit && ((Unit) target).Auras.Contains(cast.Spell)))
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
      if(TargetEvaluator == null)
      {
        Add(target);
        return Count < limit;
      }

      if(Count < limit)
      {
        Add(target);
      }
      else
      {
        int num = EvaluateTarget(target);
        int index1 = -1;
        for(int index2 = Count - 1; index2 >= 0; --index2)
        {
          int target1 = EvaluateTarget(this[index2]);
          if(target1 > num)
          {
            num = target1;
            index1 = index2;
          }
        }

        if(index1 > -1)
          this[index1] = target;
      }

      return true;
    }

    public void AddSingleTarget(WorldObject target)
    {
      if(TargetEvaluator != null)
        log.Warn(
          "Target Evaluator is not null, but only a single target adder was used - Consider using an \"AddArea*\" adder to add the best choice from any possible nearby target for spell: " +
          FirstHandler.Effect.Spell);
      Add(target);
    }

    public SpellFailedReason FindAllTargets()
    {
      IsInitialized = true;
      WorldObject casterObject = Cast.CasterObject;
      if(casterObject == null)
      {
        log.Warn("Invalid SpellCast - Tried to find targets, without Caster set: {0}",
          casterObject);
        return SpellFailedReason.Error;
      }

      SpellEffect effect = FirstHandler.Effect;
      if(effect.Spell.IsPreventionDebuff)
      {
        SpellFailedReason spellFailedReason = SpellFailedReason.Ok;
        foreach(SpellEffectHandler handler in m_handlers)
        {
          spellFailedReason = handler.ValidateAndInitializeTarget(casterObject);
          if(spellFailedReason != SpellFailedReason.Ok)
            return spellFailedReason;
        }

        Add(casterObject);
        return spellFailedReason;
      }

      SpellFailedReason spellFailedReason1 = SpellFailedReason.Ok;
      TargetDefinition targetDefinition = effect.GetTargetDefinition();
      if(targetDefinition != null)
      {
        SpellFailedReason failReason = SpellFailedReason.Ok;
        targetDefinition.Collect(this, ref failReason);
      }
      else
      {
        if(effect.ImplicitTargetA != ImplicitSpellTargetType.None)
        {
          spellFailedReason1 = FindTargets(effect.ImplicitTargetA);
          if(spellFailedReason1 != SpellFailedReason.Ok)
            return spellFailedReason1;
        }

        if(effect.ImplicitTargetB != ImplicitSpellTargetType.None)
          spellFailedReason1 = FindTargets(effect.ImplicitTargetB);
      }

      return spellFailedReason1;
    }

    public SpellFailedReason FindTargets(ImplicitSpellTargetType targetType)
    {
      SpellFailedReason failReason = SpellFailedReason.Ok;
      TargetDefinition targetDefinition = DefaultTargetDefinitions.GetTargetDefinition(targetType);
      if(targetDefinition != null)
        targetDefinition.Collect(this, ref failReason);
      return failReason;
    }

    /// <summary>
    /// Adds all chained units around the selected unit to the list
    /// </summary>
    public void FindChain(Unit first, TargetFilter filter, bool harmful, int limit)
    {
      SpellEffectHandler firstHandler = FirstHandler;
      WorldObject caster = firstHandler.Cast.CasterObject;
      Spell spell = firstHandler.Cast.Spell;
      first.IterateEnvironment(firstHandler.GetRadius(), target =>
      {
        if((spell.FacingFlags & SpellFacingFlags.RequiresInFront) != 0 && caster != null &&
           !target.IsInFrontOf(caster) || (target == caster || target == first || caster == null) ||
           (harmful && !caster.MayAttack(target) ||
            !harmful && !caster.IsInSameDivision(target) ||
            ValidateTarget(target, filter) != SpellFailedReason.Ok))
          return true;
        AddOrReplace(target, limit);
        return true;
      });
      if(TargetEvaluator != null)
        return;
      Sort((a, b) =>
        a.GetDistanceSq(first).CompareTo(b.GetDistanceSq(first)));
      if(Count <= limit)
        return;
      RemoveRange(limit, Count - limit);
    }

    /// <summary>
    /// Does default checks on whether the given Target is valid for the current SpellCast
    /// </summary>
    public SpellFailedReason ValidateTarget(WorldObject target, TargetFilter filter)
    {
      SpellEffectHandler firstHandler = FirstHandler;
      SpellCast cast = firstHandler.Cast;
      SpellFailedReason failReason = cast.Spell.CheckValidTarget(cast.CasterObject, target);
      if(failReason != SpellFailedReason.Ok)
        return failReason;
      if(filter != null)
      {
        filter(firstHandler, target, ref failReason);
        if(failReason != SpellFailedReason.Ok)
          return failReason;
      }

      return ValidateTargetForHandlers(target);
    }

    public SpellFailedReason ValidateTargetForHandlers(WorldObject target)
    {
      foreach(SpellEffectHandler handler in m_handlers)
      {
        if(!target.CheckObjType(handler.TargetType))
          return SpellFailedReason.BadTargets;
        SpellFailedReason spellFailedReason = handler.ValidateAndInitializeTarget(target);
        if(spellFailedReason != SpellFailedReason.Ok)
          return spellFailedReason;
      }

      return SpellFailedReason.Ok;
    }

    /// <summary>
    /// Removes all targets that don't satisfy the effects' constraints
    /// </summary>
    public void RevalidateAll()
    {
      SpellEffectHandler firstHandler = FirstHandler;
      SpellCast cast = firstHandler.Cast;
      for(int index = Count - 1; index >= 0; --index)
      {
        WorldObject target = this[index];
        if(target.IsInWorld)
        {
          TargetDefinition targetDefinition = firstHandler.Effect.GetTargetDefinition();
          if(targetDefinition != null)
          {
            if(ValidateTarget(target, targetDefinition.Filter) == SpellFailedReason.Ok)
              continue;
          }
          else if(ValidateTarget(target,
                    DefaultTargetDefinitions.GetTargetFilter(firstHandler.Effect.ImplicitTargetA)) ==
                  SpellFailedReason.Ok)
          {
            TargetFilter targetFilter =
              DefaultTargetDefinitions.GetTargetFilter(firstHandler.Effect.ImplicitTargetB);
            if(targetFilter != null)
            {
              SpellFailedReason failReason = SpellFailedReason.Ok;
              targetFilter(firstHandler, target, ref failReason);
              if(failReason == SpellFailedReason.Ok)
                continue;
            }
            else
              continue;
          }
        }

        RemoveAt(index);
      }
    }

    public void ApplyToAllOf<TargetType>(TargetHandler<TargetType> handler)
      where TargetType : ObjectBase
    {
      foreach(WorldObject worldObject in this)
      {
        if(worldObject is TargetType)
          handler(worldObject as TargetType);
      }
    }

    internal void Dispose()
    {
      if(!IsInitialized)
        return;
      IsInitialized = false;
      m_handlers.Clear();
      Clear();
    }

    public delegate void TargetHandler<in T>(T target);
  }
}