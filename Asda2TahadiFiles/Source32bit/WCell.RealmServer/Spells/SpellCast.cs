using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Misc;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.NLog;
using WCell.Util.ObjectPools;
using WCell.Util.Threading;

namespace WCell.RealmServer.Spells
{
  /// <summary>Represents the progress of any Spell-casting</summary>
  public class SpellCast : IUpdatable, IWorldLocation, IHasPosition
  {
    public static int PushbackDelay = 500;
    public static int ChannelPushbackFraction = 4;

    internal static readonly ObjectPool<SpellCast> SpellCastPool =
      ObjectPoolMgr.CreatePool(() => new SpellCast(), true);

    public static readonly ObjectPool<List<IAura>> AuraListPool =
      ObjectPoolMgr.CreatePool(() => new List<IAura>(), true);

    public static readonly ObjectPool<List<MissedTarget>> CastMissListPool =
      ObjectPoolMgr.CreatePool(() => new List<MissedTarget>(3),
        true);

    public static readonly ObjectPool<List<SpellEffectHandler>> SpellEffectHandlerListPool =
      ObjectPoolMgr.CreatePool(
        () => new List<SpellEffectHandler>(3), true);

    private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<Unit, ProcHitFlags> m_hitInfoByTarget = new Dictionary<Unit, ProcHitFlags>();
    private readonly SpellHitChecker hitChecker = new SpellHitChecker();
    private int m_castDelay;
    private List<AuraApplicationInfo> m_auraApplicationInfos;
    private Vector3 m_targetLoc;
    private readonly TimerEntry m_castTimer;

    /// <summary>
    /// The amount of Pushbacks (the more Pushbacks, the less effective they are)
    /// </summary>
    private int m_pushbacks;

    /// <summary>Called during Preparation</summary>
    private SpellFailedReason PrepareAI()
    {
      Unit casterUnit = CasterUnit;
      SourceLoc = casterUnit.Position;
      if(casterUnit.Target != null)
        casterUnit.SpellCast.TargetLoc = casterUnit.Target.Position;
      return SpellFailedReason.Ok;
    }

    private bool PrePerformAI()
    {
      return true;
    }

    private void RevalidateAllTargets()
    {
      Targets.Clear();
      HashSet<SpellTargetCollection> targetCollectionSet = new HashSet<SpellTargetCollection>();
      foreach(SpellEffectHandler handler in Handlers)
        targetCollectionSet.Add(handler.Targets);
      foreach(SpellTargetCollection targetCollection in targetCollectionSet)
      {
        targetCollection.RevalidateAll();
        Targets.AddRange(targetCollection);
      }
    }

    /// <summary>Called when finished casting</summary>
    private void OnAICasted()
    {
      if(Spell.AISettings.IdleTimeAfterCastMillis <= 0)
        return;
      CasterUnit.Idle(Spell.AISettings.IdleTimeAfterCastMillis);
    }

    /// <summary>
    /// Checks whether the given target resisted the debuff, represented through the given spell
    /// </summary>
    public static CastMissReason CheckDebuffResist(Unit target, Spell spell, int casterLevel, bool hostile)
    {
      CastMissReason castMissReason = CastMissReason.None;
      if(hostile && target.CheckDebuffResist(casterLevel, target.GetLeastResistantSchool(spell)))
        castMissReason = CastMissReason.Resist;
      return castMissReason;
    }

    private SpellFailedReason PrepAuras()
    {
      m_auraApplicationInfos = new List<AuraApplicationInfo>(4);
      SpellEffectHandler spellEffectHandler = null;
      for(int index = 0; index < Handlers.Length; ++index)
      {
        SpellEffectHandler handler = Handlers[index];
        if(handler.Effect.IsAuraEffect && (spellEffectHandler == null ||
                                           !spellEffectHandler.Effect.SharesTargetsWith(handler.Effect)))
        {
          spellEffectHandler = handler;
          if(handler.m_targets != null)
          {
            foreach(WorldObject target1 in handler.m_targets)
            {
              WorldObject target = target1;
              if(target is Unit &&
                 !m_auraApplicationInfos.Any(
                   info => info.Target == target))
              {
                AuraIndexId auraUid = Spell.GetAuraUID(CasterReference, target);
                SpellFailedReason err = SpellFailedReason.Ok;
                if(((Unit) target).Auras.PrepareStackOrOverride(CasterReference, auraUid,
                  Spell, ref err, this))
                  m_auraApplicationInfos.Add(new AuraApplicationInfo((Unit) target));
                else if(err != SpellFailedReason.Ok && !IsAoE)
                  return err;
              }
            }
          }
        }
      }

      return SpellFailedReason.Ok;
    }

    private void CreateAuras(ref List<MissedTarget> missedTargets, ref List<IAura> auras, DynamicObject dynObj)
    {
      auras = AuraListPool.Obtain();
      bool persistsThroughDeath = Spell.PersistsThroughDeath;
      if(Spell.IsAreaAura)
      {
        if(dynObj != null || CasterObject != null &&
           (persistsThroughDeath || !(CasterObject is Unit) || ((Unit) CasterObject).IsAlive))
        {
          AreaAura areaAura = new AreaAura(dynObj ?? CasterObject, Spell);
          if(dynObj != null)
            auras.Add(areaAura);
        }
        else
          LogManager.GetCurrentClassLogger().Warn(
            "Tried to cast Spell {0} with invalid dynObj or Caster - dynObj: {1}, CasterObject: {2}, CasterUnit: {3}",
            (object) Spell, (object) dynObj, (object) CasterObject, (object) CasterUnit);
      }

      for(int index = m_auraApplicationInfos.Count - 1; index >= 0; --index)
      {
        if(!Targets.Contains(m_auraApplicationInfos[index].Target))
          m_auraApplicationInfos.RemoveAt(index);
      }

      if(m_auraApplicationInfos.Count == 0)
        return;
      for(int index = 0; index < Handlers.Length; ++index)
      {
        SpellEffectHandler handler = Handlers[index];
        if(handler is ApplyAuraEffectHandler)
          ((ApplyAuraEffectHandler) handler).AddAuraHandlers(m_auraApplicationInfos);
      }

      if(missedTargets == null)
        missedTargets = CastMissListPool.Obtain();
      for(int index = 0; index < m_auraApplicationInfos.Count; ++index)
      {
        AuraApplicationInfo auraApplicationInfo = m_auraApplicationInfos[index];
        Unit target = auraApplicationInfo.Target;
        if(target.IsInContext && auraApplicationInfo.Handlers != null &&
           (persistsThroughDeath || target.IsAlive))
        {
          bool hostile = Spell.IsHarmfulFor(CasterReference, target);
          CastMissReason reason;
          if(!IsPassive && !Spell.IsPreventionDebuff &&
             (reason = CheckDebuffResist(target, Spell, CasterReference.Level,
               hostile)) != CastMissReason.None)
          {
            missedTargets.Add(new MissedTarget(target, reason));
          }
          else
          {
            Aura aura = target.Auras.CreateAura(CasterReference, Spell,
              auraApplicationInfo.Handlers, TargetItem, !Spell.IsPreventionDebuff && !hostile);
            if(aura != null)
            {
              if(!Spell.IsPreventionDebuff &&
                 (Spell.AttributesExC & SpellAttributesExC.NoInitialAggro) ==
                 SpellAttributesExC.None && (hostile && target.IsInWorld) && target.IsAlive)
              {
                target.IsInCombat = true;
                if(target is NPC && CasterUnit != null)
                  ((NPC) target).ThreatCollection.AddNewIfNotExisted(CasterUnit);
              }

              auras.Add(aura);
            }
          }
        }
      }

      m_auraApplicationInfos = null;
    }

    private SpellFailedReason PrepareHandlers()
    {
      SpellFailedReason failReason = SpellFailedReason.Ok;
      SpellEffectHandler[] handlers = CreateHandlers(ref failReason);
      if(failReason != SpellFailedReason.Ok)
        return failReason;
      Handlers = handlers;
      SpellFailedReason spellFailedReason = InitializeHandlers();
      if(spellFailedReason != SpellFailedReason.Ok)
        return spellFailedReason;
      return InitializeHandlersTargets();
    }

    private SpellEffectHandler[] CreateHandlers(ref SpellFailedReason failReason)
    {
      SpellEffectHandler[] handlers = new SpellEffectHandler[Spell.EffectHandlerCount];
      int h = 0;
      SpellTargetCollection targets = null;
      foreach(SpellEffect effect in Spell.Effects.Where(
        effect => effect.SpellEffectHandlerCreator != null))
      {
        CreateHandler(effect, h, handlers, ref targets, ref failReason);
        if(failReason != SpellFailedReason.Ok)
          return null;
        ++h;
      }

      return handlers;
    }

    private void CreateHandler(SpellEffect effect, int h, SpellEffectHandler[] handlers,
      ref SpellTargetCollection targets, ref SpellFailedReason failReason)
    {
      SpellEffectHandler spellEffectHandler = effect.SpellEffectHandlerCreator(this, effect);
      handlers[h] = spellEffectHandler;
      spellEffectHandler.InitialTarget = (Unit) SelectedTarget;
      if(targets == null)
        targets = CreateSpellTargetCollection();
      if(targets == null)
        return;
      spellEffectHandler.m_targets = targets;
      targets.m_handlers.Add(spellEffectHandler);
    }

    private SpellFailedReason InitializeHandlers()
    {
      foreach(SpellEffectHandler handler in Handlers)
      {
        SpellFailedReason spellFailedReason = handler.Initialize();
        if(spellFailedReason != SpellFailedReason.Ok)
        {
          Handlers = null;
          return spellFailedReason;
        }
      }

      return SpellFailedReason.Ok;
    }

    private SpellFailedReason InitializeHandlersTargets()
    {
      foreach(SpellEffectHandler handler in Handlers
        .Where(handler =>
        {
          if(handler.Targets != null)
            return !handler.Targets.IsInitialized;
          return false;
        }))
      {
        SpellFailedReason spellFailedReason = CollectHandlerTargets(handler);
        if(spellFailedReason != SpellFailedReason.Ok)
          return spellFailedReason;
      }

      return SpellFailedReason.Ok;
    }

    private SpellFailedReason CollectHandlerTargets(SpellEffectHandler handler)
    {
      SpellFailedReason spellFailedReason = InitialTargets != null
        ? handler.Targets.AddAll(InitialTargets)
        : handler.Targets.FindAllTargets();
      if(spellFailedReason != SpellFailedReason.Ok)
        return spellFailedReason;
      AddHandlerTargetsToTargets(handler);
      return SpellFailedReason.Ok;
    }

    private void AddHandlerTargetsToTargets(SpellEffectHandler handler)
    {
      foreach(WorldObject target in handler.Targets)
        Targets.Add(target);
    }

    private SpellTargetCollection CreateSpellTargetCollection()
    {
      return SpellTargetCollection.Obtain();
    }

    private void Perform(int elapsed)
    {
      CheckCasterValidity();
      int num = (int) Perform();
    }

    /// <summary>
    /// Does some sanity checks and adjustments right before perform
    /// </summary>
    protected SpellFailedReason PrePerform()
    {
      if(IsPlayerCast)
      {
        SpellFailedReason spellFailedReason = PlayerPrePerform();
        if(spellFailedReason != SpellFailedReason.Ok)
          return spellFailedReason;
      }

      if(CasterUnit == null && Spell.IsChanneled)
        return SpellFailedReason.CasterAurastate;
      if(Spell.IsAura)
      {
        if(Targets.Count == 0 && !IsAoE && !Spell.IsAreaAura)
          return SpellFailedReason.NoValidTargets;
        SpellFailedReason reason = PrepAuras();
        if(reason != SpellFailedReason.Ok)
        {
          Cancel(reason);
          return reason;
        }
      }

      if(CasterUnit != null)
      {
        if(SelectedLock != null && SelectedLock.RequiresKneeling)
          CasterUnit.StandState = StandState.Kneeling;
        CancelStealth();
      }

      return IsAICast && !PrePerformAI() ? SpellFailedReason.NoValidTargets : SpellFailedReason.Ok;
    }

    private SpellFailedReason PlayerPrePerform()
    {
      if(Spell.TargetFlags.HasAnyFlag(SpellTargetFlags.Item))
      {
        SpellFailedReason spellFailedReason = CheckTargetItem();
        if(spellFailedReason != SpellFailedReason.Ok)
          return spellFailedReason;
      }

      if(CastFailsDueToImmune)
      {
        Cancel(SpellFailedReason.Immune);
        return SpellFailedReason.Immune;
      }

      if(Spell.IsAutoRepeating)
        return ToggleAutorepeatingSpell();
      if(Spell.Attributes.HasFlag(SpellAttributes.StopsAutoAttack))
        StopAutoAttack();
      return SpellFailedReason.Ok;
    }

    private SpellFailedReason CheckTargetItem()
    {
      if(!ItemIsSelected)
      {
        LogWarnIfIsPassive("Trying to trigger Spell without Item selected: " + this);
        return SpellFailedReason.ItemNotFound;
      }

      if(ItemIsReady)
        return SpellFailedReason.Ok;
      LogWarnIfIsPassive("Trying to trigger Spell without Item ready: " + this);
      return SpellFailedReason.ItemNotReady;
    }

    private bool ItemIsSelected
    {
      get
      {
        if(TargetItem != null && TargetItem.IsInWorld)
          return TargetItem.Owner == CasterObject;
        return false;
      }
    }

    private void LogWarnIfIsPassive(string message)
    {
      if(!IsPassive)
        return;
      _log.Warn(message);
    }

    private bool ItemIsReady
    {
      get
      {
        if(TargetItem.IsEquipped)
          return TargetItem.Unequip();
        return true;
      }
    }

    private bool CastFailsDueToImmune
    {
      get
      {
        if(CastCanFailDueToImmune)
          return SelectedUnitIsImmuneToSpell;
        return false;
      }
    }

    private bool CastCanFailDueToImmune
    {
      get
      {
        if(!IsAoE && SelectedTarget is Unit)
          return !Spell.IsPreventionDebuff;
        return false;
      }
    }

    private bool SelectedUnitIsImmuneToSpell
    {
      get
      {
        bool flag = Spell.IsHarmfulFor(CasterReference, SelectedTarget);
        Unit selectedTarget = (Unit) SelectedTarget;
        if(flag)
          return selectedTarget.IsImmuneToSpell(Spell);
        return false;
      }
    }

    private SpellFailedReason ToggleAutorepeatingSpell()
    {
      if(CasterUnit.Target == null && !(SelectedTarget is Unit))
        return SpellFailedReason.BadTargets;
      CasterUnit.IsFighting = true;
      if(CasterUnit.AutorepeatSpell == Spell)
        DeactivateAutorepeatingSpell();
      else
        ActivateAutorepeatingSpell();
      return SpellFailedReason.DontReport;
    }

    private void ActivateAutorepeatingSpell()
    {
      CasterUnit.AutorepeatSpell = Spell;
      SendCastStart();
    }

    private void DeactivateAutorepeatingSpell()
    {
      CasterUnit.AutorepeatSpell = null;
    }

    private void StopAutoAttack()
    {
      DeactivateAutorepeatingSpell();
      CasterUnit.IsFighting = false;
    }

    private void CancelStealth()
    {
      if(Spell.AttributesEx.HasFlag(SpellAttributesEx.RemainStealthed))
        return;
      CasterUnit.Auras.RemoveWhere(aura => aura.Spell.DispelType == DispelType.Stealth);
    }

    private LockEntry SelectedLock
    {
      get { return (SelectedTarget as ILockable)?.Lock; }
    }

    /// <summary>Performs the actual Spell</summary>
    internal SpellFailedReason Perform()
    {
      try
      {
        if(Handlers == null)
        {
          SpellFailedReason reason = PrepareHandlers();
          if(reason != SpellFailedReason.Ok)
          {
            Cancel(reason);
            return reason;
          }
        }

        SpellFailedReason reason1 = PrePerform();
        if(reason1 != SpellFailedReason.Ok)
        {
          Cancel(reason1);
          return reason1;
        }

        SpellFailedReason spellFailedReason = Impact();
        if(IsCasting && CasterUnit != null)
          OnUnitCasted();
        if(IsCasting && !IsChanneling)
          Cleanup();
        return spellFailedReason;
      }
      catch(Exception ex)
      {
        OnException(ex);
        return SpellFailedReason.Error;
      }
    }

    /// <summary>
    /// Calculates the delay until a spell impacts its target in milliseconds
    /// </summary>
    /// <returns>delay in ms</returns>
    private int CalculateImpactDelay()
    {
      if(CasterChar != null)
        return 0;
      return (int) Spell.CastDelay;
    }

    private void DoDelayedImpact(int delay)
    {
      if(CasterObject != null)
      {
        CasterObject.CallDelayed(delay, DelayedImpact);
        if(Spell.IsChanneled || this != CasterObject.SpellCast)
          return;
        CasterObject.SpellCast = null;
      }
      else
        Map.CallDelayed(delay, () => DelayedImpact(null));
    }

    private void DelayedImpact(WorldObject obj)
    {
      CheckCasterValidity();
      foreach(WorldObject target in Targets.Where(
        target => !target.IsInWorld))
        Remove(target);
      try
      {
        int num = (int) Impact();
        if(!Spell.IsChanneled && IsCasting)
          Cleanup();
        WorldObject casterObject = CasterObject;
        if(casterObject == null || casterObject.SpellCast != null || IsPassive)
          return;
        casterObject.SpellCast = this;
      }
      catch(Exception ex)
      {
        OnException(ex);
      }
    }

    /// <summary>Validates targets and applies all SpellEffects</summary>
    public SpellFailedReason Impact()
    {
      if(!IsCasting)
        return SpellFailedReason.Ok;
      foreach(SpellEffectHandler handler in Handlers)
      {
        if(!handler.Effect.IsPeriodic && !handler.Effect.IsStrikeEffect)
        {
          handler.Apply();
          if(!IsCasting)
            return SpellFailedReason.DontReport;
        }
      }

      if(CasterObject is Unit && Spell.IsPhysicalAbility)
      {
        foreach(Unit unitTarget in UnitTargets)
        {
          ProcHitFlags procHitFlags = CasterUnit.Strike(GetWeapon(), unitTarget, this);
          m_hitInfoByTarget[unitTarget] = procHitFlags;
        }
      }

      DynamicObject dynObj = null;
      if(Spell.DOEffect != null)
        dynObj = new DynamicObject(this, Spell.DOEffect.GetRadius(CasterReference));
      if(!IsCasting)
        return SpellFailedReason.Ok;
      List<MissedTarget> missedTargets = null;
      List<IAura> auras = null;
      if(m_auraApplicationInfos != null)
        CreateAuras(ref missedTargets, ref auras, dynObj);
      if(missedTargets != null)
      {
        if(missedTargets.Count > 0)
        {
          CombatLogHandler.SendSpellMiss(this, true, missedTargets);
          missedTargets.Clear();
        }

        CastMissListPool.Recycle(missedTargets);
      }

      if(Spell.IsChanneled && CasterObject != null)
      {
        Channel = SpellChannel.SpellChannelPool.Obtain();
        Channel.m_cast = this;
        if(CasterObject is Unit)
        {
          if(dynObj != null)
            CasterUnit.ChannelObject = dynObj;
          else if(SelectedTarget != null)
          {
            CasterUnit.ChannelObject = SelectedTarget;
            if(SelectedTarget is NPC && Spell.IsTame)
              ((NPC) SelectedTarget).CurrentTamer = CasterObject as Character;
          }
        }

        int length = Handlers.Length;
        List<SpellEffectHandler> channelHandlers = SpellEffectHandlerListPool.Obtain();
        for(int index = 0; index < length; ++index)
        {
          SpellEffectHandler handler = Handlers[index];
          if(handler.Effect.IsPeriodic)
            channelHandlers.Add(handler);
        }

        Channel.Open(channelHandlers, auras);
      }

      if(auras != null)
      {
        for(int index = 0; index < auras.Count; ++index)
          auras[index].Start(Spell.IsChanneled ? Channel : null, false);
        if(!IsChanneling)
        {
          auras.Clear();
          AuraListPool.Recycle(auras);
          auras = null;
        }
      }

      if(Spell.HasHarmfulEffects && !Spell.IsPreventionDebuff)
      {
        foreach(WorldObject target in Targets)
        {
          if(target is Unit && Spell.IsHarmfulFor(CasterReference, target))
            ((Unit) target).Auras.RemoveByFlag(AuraInterruptFlags.OnHostileSpellInflicted);
        }
      }

      return SpellFailedReason.Ok;
    }

    protected void OnUnitCasted()
    {
      OnAliveUnitCasted();
      OnTargetItemUsed();
      UpdateAuraState();
      if(!GodMode)
      {
        OnNonGodModeSpellCasted();
        if(!IsCasting)
          return;
      }
      else if(!IsPassive && CasterUnit is Character)
        ClearCooldowns();

      AddRunicPower();
      TriggerSpellsAfterCastingSpells();
      if(!IsCasting)
        return;
      TriggerDynamicPostCastSpells();
      if(!IsCasting)
        return;
      ConsumeCombopoints();
      ConsumeSpellModifiers();
      if(!IsCasting)
        return;
      if(IsAICast)
      {
        OnAICasted();
        if(!IsCasting)
          return;
      }

      Spell.NotifyCasted(this);
      if(CasterUnit is Character)
        CasterChar.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CastSpell,
          Spell.Id, 0U, null);
      TriggerProcOnCasted();
      m_hitInfoByTarget.Clear();
    }

    private void OnAliveUnitCasted()
    {
      if(!CasterUnit.IsAlive)
        return;
      if(CasterUnit is Character)
        OnAliveCharacterCasted();
      PutCasterInCombatModeAfterCastOnCombatant();
      ResetSwingDelay();
    }

    private void OnAliveCharacterCasted()
    {
      SitWhileConsuming();
      GainSkill();
      if(UsedCombatAbility)
        OnCharacterCombatAbilityUsed();
      CheckForQuestProgress();
    }

    private void SitWhileConsuming()
    {
      if(!Spell.IsFood && !Spell.IsDrink)
        return;
      CasterChar.StandState = StandState.Sit;
      if(!Spell.IsFood)
        return;
      CasterChar.Emote(EmoteType.SimpleEat);
    }

    private void GainSkill()
    {
      if(Spell.Ability == null || !Spell.Ability.CanGainSkill)
        return;
      Skill skill = CasterChar.Skills[Spell.Ability.Skill.Id];
      ushort currentValue = skill.CurrentValue;
      ushort actualMax = (ushort) skill.ActualMax;
      if(currentValue >= actualMax)
        return;
      ushort num = (ushort) (currentValue + (uint) (ushort) Spell.Ability.Gain(currentValue));
      skill.CurrentValue = (int) num <= (int) actualMax ? num : actualMax;
    }

    private bool UsedCombatAbility
    {
      get
      {
        if(Spell.IsPhysicalAbility)
          return Spell.IsRangedAbility;
        return false;
      }
    }

    private void OnCharacterCombatAbilityUsed()
    {
    }

    private void CheckForQuestProgress()
    {
      CasterChar.QuestLog.OnSpellCast(this);
    }

    private void PutCasterInCombatModeAfterCastOnCombatant()
    {
      if(CasterUnit.IsInCombat ||
         UnitTargets.Where(target => target.IsInCombat).Count() <= 0)
        return;
      CasterUnit.IsInCombat = true;
    }

    private void ResetSwingDelay()
    {
      if(!Spell.HasHarmfulEffects || Spell.IsPreventionDebuff || !CasterUnit.IsInCombat)
        return;
      CasterUnit.ResetSwingDelay();
    }

    private void OnTargetItemUsed()
    {
      if(TargetItem == null)
        return;
      CasterChar.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.UseItem, Spell.Id,
        0U, null);
      TargetItem.OnUse();
    }

    private void ConsumeSpellModifiers()
    {
      CasterUnit.Auras.OnCasted(this);
    }

    private void TriggerDynamicPostCastSpells()
    {
      CasterUnit.Spells.TriggerSpellsFor(this);
    }

    private void ConsumeCombopoints()
    {
      if(!Spell.IsFinishingMove)
        return;
      CasterUnit.ModComboState(null, 0);
    }

    private void TriggerSpellsAfterCastingSpells()
    {
      TriggerTargetTriggerSpells();
      if(!IsCasting)
        return;
      TriggerCasterTriggerSpells();
    }

    private void TriggerCasterTriggerSpells()
    {
      if(Spell.CasterTriggerSpells == null)
        return;
      foreach(Spell casterTriggerSpell in Spell.CasterTriggerSpells)
      {
        Trigger(casterTriggerSpell, Targets.ToArray());
        if(!IsCasting)
          break;
      }
    }

    private void TriggerTargetTriggerSpells()
    {
      if(Spell.TargetTriggerSpells == null)
        return;
      foreach(Spell targetTriggerSpell in Spell.TargetTriggerSpells)
      {
        Trigger(targetTriggerSpell, Targets.ToArray());
        if(!IsCasting)
          break;
      }
    }

    private void AddRunicPower()
    {
      if(!UsesRunes)
        return;
      CasterUnit.Power += Spell.RuneCostEntry.RunicPowerGain;
    }

    private void OnNonGodModeSpellCasted()
    {
      AddCooldown();
      ConsumeRunes();
      ConsumePower();
    }

    private void ClearCooldowns()
    {
      PlayerSpellCollection playerSpells = CasterChar.PlayerSpells;
      if(playerSpells == null)
        return;
      playerSpells.ClearCooldown(Spell, true);
    }

    private void ConsumePower()
    {
      int num = Spell.CalcPowerCost(CasterUnit,
        SelectedTarget is Unit
          ? ((Unit) SelectedTarget).GetLeastResistantSchool(Spell)
          : Spell.Schools[0]);
      if(Spell.PowerType != PowerType.Health)
        CasterUnit.Power -= num;
      else
        CasterUnit.Health -= num;
    }

    private void ConsumeRunes()
    {
      if(!UsesRunes)
        return;
      CasterChar.PlayerSpells.Runes.ConsumeRunes(Spell);
    }

    private void AddCooldown()
    {
      if(!Spell.IsAutoRepeating && TriggerEffect == null)
        CasterUnit.Spells.AddCooldown(Spell, CasterItem);
      if(Client == null ||
         Spell.Attributes.HasFlag(SpellAttributes.StartCooldownAfterEffectFade) ||
         CasterItem == null)
        return;
      SpellHandler.SendItemCooldown(Client, Spell.Id, CasterItem);
    }

    private void UpdateAuraState()
    {
      if(Spell.RequiredCasterAuraState != AuraState.DodgeOrBlockOrParry)
        return;
      CasterUnit.AuraState &= ~AuraStateMask.DodgeOrBlockOrParry;
    }

    private void TriggerProcOnCasted()
    {
      ProcTriggerFlags flags1 = ProcTriggerFlags.None;
      ProcTriggerFlags flags2 = ProcTriggerFlags.None;
      switch(Spell.DamageType)
      {
        case DamageType.None:
          if(Spell.IsBeneficial)
          {
            flags1 |= ProcTriggerFlags.DoneBeneficialSpell;
            flags2 |= ProcTriggerFlags.ReceivedBeneficialSpell;
            break;
          }

          if(Spell.IsHarmful)
          {
            flags1 |= ProcTriggerFlags.DoneHarmfulSpell;
            flags2 |= ProcTriggerFlags.ReceivedHarmfulSpell;
          }

          break;
        case DamageType.Magic:
          if(Spell.IsBeneficial)
          {
            flags1 |= ProcTriggerFlags.DoneBeneficialMagicSpell;
            flags2 |= ProcTriggerFlags.ReceivedBeneficialMagicSpell;
            break;
          }

          if(Spell.IsHarmful)
          {
            flags1 |= ProcTriggerFlags.DoneHarmfulMagicSpell;
            flags2 |= ProcTriggerFlags.ReceivedHarmfulMagicSpell;
          }

          break;
        case DamageType.Melee:
          flags1 |= ProcTriggerFlags.DoneMeleeSpell;
          flags2 |= ProcTriggerFlags.ReceivedMeleeSpell;
          break;
        case DamageType.Ranged:
          if(Spell.IsAutoRepeating)
          {
            flags1 |= ProcTriggerFlags.DoneRangedAutoAttack;
            flags2 |= ProcTriggerFlags.ReceivedRangedAutoAttack;
            break;
          }

          flags1 |= ProcTriggerFlags.DoneRangedSpell;
          flags2 |= ProcTriggerFlags.ReceivedRangedSpell;
          break;
      }

      ProcHitFlags hitFlags = TriggerProcOnTargets(flags2);
      TriggerProcOnCaster(flags1, hitFlags);
    }

    /// <summary>Triggers proc on all targets of SpellCast</summary>
    /// <param name="flags">What happened to targets ie. ProcTriggerFlags.ReceivedHarmfulSpell</param>
    /// <returns>Combination of hit result on all targets.</returns>
    private ProcHitFlags TriggerProcOnTargets(ProcTriggerFlags flags)
    {
      ProcHitFlags procHitFlags1 = ProcHitFlags.None;
      foreach(KeyValuePair<Unit, ProcHitFlags> keyValuePair in m_hitInfoByTarget)
      {
        Unit key = keyValuePair.Key;
        ProcHitFlags procHitFlags2 = keyValuePair.Value;
        procHitFlags1 |= procHitFlags2;
        SimpleUnitAction simpleUnitAction = new SimpleUnitAction
        {
          Attacker = CasterUnit,
          Spell = Spell,
          Victim = key,
          IsCritical = procHitFlags2.HasAnyFlag(ProcHitFlags.CriticalHit)
        };
        key.Proc(flags, CasterUnit, simpleUnitAction, true, procHitFlags2);
      }

      return procHitFlags1;
    }

    /// <summary>Trigger proc on the caster of the spell.</summary>
    /// <param name="flags">What spell caster casted ie. ProcTriggerFlags.DoneHarmfulSpell</param>
    /// <param name="hitFlags">Hit result of the spell</param>
    private void TriggerProcOnCaster(ProcTriggerFlags flags, ProcHitFlags hitFlags)
    {
      SimpleUnitAction simpleUnitAction = new SimpleUnitAction
      {
        Attacker = CasterUnit,
        Spell = Spell,
        Victim = m_hitInfoByTarget.Count > 0
          ? m_hitInfoByTarget.First().Key
          : null,
        IsCritical = hitFlags.HasAnyFlag(ProcHitFlags.CriticalHit)
      };
      Unit triggerer = UnitTargets.FirstOrDefault();
      CasterUnit.Proc(flags, triggerer, simpleUnitAction, true, hitFlags);
    }

    /// <summary>Spell being casted</summary>
    public Spell Spell { get; private set; }

    /// <summary>All SpellEffectHandlers</summary>
    public SpellEffectHandler[] Handlers { get; private set; }

    /// <summary>
    /// Something that has been selected by the Caster for this Spell
    /// </summary>
    public WorldObject SelectedTarget { get; set; }

    /// <summary>Returns all targets that this SpellCast initially had</summary>
    public WorldObject[] InitialTargets { get; private set; }

    public HashSet<WorldObject> Targets { get; private set; }

    private IEnumerable<Unit> UnitTargets
    {
      get { return Targets.OfType<Unit>(); }
    }

    public SpellTargetFlags TargetFlags { get; set; }

    public Map TargetMap
    {
      get { return Spell.TargetLocation == null ? Map : Spell.TargetLocation.Map ?? Map; }
    }

    /// <summary>
    /// The target location for a spell which has been sent by the player
    /// </summary>
    public Vector3 TargetLoc
    {
      get { return m_targetLoc; }
      set { m_targetLoc = value; }
    }

    public float TargetOrientation
    {
      get
      {
        if(Spell.TargetLocation != null || CasterObject == null)
          return Spell.TargetOrientation;
        return CasterObject.Orientation;
      }
    }

    /// <summary>An Item that this Spell is being used on</summary>
    public Item TargetItem { get; set; }

    public string StringTarget { get; set; }

    public ObjectReference CasterReference { get; internal set; }

    /// <summary>
    /// The Unit or GameObject (traps etc), triggering this spell
    /// </summary>
    public WorldObject CasterObject { get; private set; }

    /// <summary>
    /// The caster himself or owner of the casting Item or GameObject
    /// </summary>
    public Unit CasterUnit { get; private set; }

    /// <summary>
    /// The caster himself or owner of the casting Item or GameObject
    /// </summary>
    public Character CasterChar
    {
      get { return CasterUnit as Character; }
    }

    /// <summary>
    /// This corresponds to the actual level of Units
    /// and for GOs returns the level of the owner.
    /// </summary>
    public int CasterLevel
    {
      get { return CasterReference.Level; }
    }

    /// <summary>
    /// Any kind of item that was used to trigger this cast
    /// (trinkets, potions, food etc.)
    /// </summary>
    public Item CasterItem { get; set; }

    /// <summary>
    /// The source location for a spell which has been sent by the player
    /// </summary>
    public Vector3 SourceLoc { get; set; }

    /// <summary>The Caster's or Caster's Master's Client (or null)</summary>
    public IRealmClient Client
    {
      get { return (CasterUnit as Character)?.Client; }
    }

    /// <summary>The map where the SpellCast happens</summary>
    public Map Map { get; internal set; }

    /// <summary>Needed for IWorldLocation interface</summary>
    public MapId MapId
    {
      get { return Map.MapId; }
    }

    /// <summary>Needed for IWorldLocation interface</summary>
    public Vector3 Position
    {
      get { return SourceLoc; }
    }

    /// <summary>The context to which the SpellCast belongs</summary>
    public IContextHandler Context
    {
      get { return Map; }
    }

    public uint Phase { get; internal set; }

    public CastFlags StartFlags
    {
      get
      {
        CastFlags castFlags = CastFlags.None;
        if(Spell != null)
        {
          if(Spell.IsRangedAbility)
            castFlags |= CastFlags.Ranged;
          if(UsesRunes)
            castFlags |= CastFlags.RuneAbility;
        }

        return castFlags;
      }
    }

    public CastFlags GoFlags
    {
      get
      {
        CastFlags castFlags = CastFlags.Flag_0x2;
        if(Spell.IsRangedAbility)
          castFlags |= CastFlags.Ranged;
        if(UsesRunes)
        {
          castFlags |= CastFlags.RuneAbility;
          if(Spell.RuneCostEntry.RunicPowerGain > 0)
            castFlags |= CastFlags.RunicPowerGain;
          if(Spell.RuneCostEntry.CostsRunes)
            castFlags |= CastFlags.RuneCooldownList;
        }

        return castFlags;
      }
    }

    /// <summary>
    /// The time at which the cast started (in millis since system start)
    /// </summary>
    public int StartTime { get; private set; }

    /// <summary>
    /// Time in milliseconds that it takes until the spell will start (0 if GodMode)
    /// </summary>
    public int CastDelay
    {
      get
      {
        if(!GodMode)
          return m_castDelay;
        return 1;
      }
    }

    /// <summary>
    /// The time in milliseconds between now and the actual casting (meaningless if smaller equal 0).
    /// Can be changed. Might return bogus numbers if not casting.
    /// </summary>
    public int RemainingCastTime
    {
      get { return CastDelay + StartTime - Environment.TickCount; }
      set
      {
        int delay = Math.Max(0, value - RemainingCastTime);
        StartTime = Environment.TickCount + delay;
        m_castTimer.RemainingInitialDelayMillis = value;
        SpellHandler.SendCastDelayed(this, delay);
      }
    }

    /// <summary>
    /// An object representing the channeling of a spell (any spell that is performed over a period of time)
    /// </summary>
    public SpellChannel Channel { get; private set; }

    public uint GlyphSlot { get; set; }

    /// <summary>
    /// Whether the SpellCast was started by an AI-controlled Unit
    /// </summary>
    public bool IsAICast
    {
      get
      {
        if(IsPlayerCast || IsPassive)
          return false;
        if(CasterUnit != null)
          return !CasterUnit.IsPlayer;
        return true;
      }
    }

    /// <summary>Whether the SpellCast was started by a Player</summary>
    public bool IsPlayerCast { get; private set; }

    /// <summary>whether the cast is currently being performed</summary>
    public bool IsCasting { get; private set; }

    /// <summary>whether the caster is currently channeling a spell</summary>
    public bool IsChanneling
    {
      get
      {
        if(Channel != null)
          return Channel.IsChanneling;
        return false;
      }
    }

    /// <summary>
    /// Whether this SpellCast is waiting to be casted on next strike
    /// </summary>
    public bool IsPending
    {
      get
      {
        if(IsCasting)
          return Spell.IsOnNextStrike;
        return false;
      }
    }

    /// <summary>
    /// Returns false if Player actively casted the spell, else true.
    /// Passive SpellCasts wont do any of the requirement checks.
    /// </summary>
    public bool IsPassive { get; private set; }

    public bool IsInstant
    {
      get
      {
        if(!IsPassive && !GodMode)
          return m_castDelay < 100;
        return true;
      }
    }

    public bool IsAoE
    {
      get
      {
        if(TriggerEffect == null)
          return Spell.IsAreaSpell;
        return TriggerEffect.IsAreaEffect;
      }
    }

    public bool UsesRunes
    {
      get
      {
        if(Spell.RuneCostEntry != null && CasterChar != null)
          return CasterChar.PlayerSpells.Runes != null;
        return false;
      }
    }

    /// <summary>Ignore most limitations</summary>
    public bool GodMode { get; set; }

    /// <summary>
    /// The SpellEffect that triggered this cast (or null if not triggered)
    /// </summary>
    public SpellEffect TriggerEffect { get; private set; }

    /// <summary>
    /// The action that triggered this SpellCast, if any.
    /// If you want to save the Action for a point later in time, you need to
    /// increment the ReferenceCount, and decrement it when you are done with it.
    /// </summary>
    public IUnitAction TriggerAction { get; private set; }

    /// <summary>Creates a recyclable SpellCast.</summary>
    private SpellCast()
    {
      m_castTimer = new TimerEntry(Perform);
      Targets = new HashSet<WorldObject>();
    }

    public static SpellCast ObtainPooledCast(WorldObject caster)
    {
      SpellCast spellCast = SpellCastPool.Obtain();
      spellCast.SetCaster(caster);
      return spellCast;
    }

    private void SetCaster(WorldObject caster)
    {
      CasterReference = caster.SharedReference;
      CasterObject = caster;
      CasterUnit = caster.UnitMaster;
      Map = caster.Map;
      Phase = caster.Phase;
    }

    private void SetCaster(ObjectReference caster, Map map, uint phase, Vector3 sourceLoc)
    {
      CasterReference = caster;
      if(caster == null)
        throw new ArgumentNullException(nameof(caster));
      CasterObject = caster.Object;
      CasterUnit = caster.UnitMaster;
      Map = map;
      Phase = phase;
      SourceLoc = sourceLoc;
    }

    public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc)
    {
      Trigger(caster, spell, ref targetLoc, null);
    }

    public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc, WorldObject selected)
    {
      Trigger(caster, spell, ref targetLoc, selected, null);
    }

    public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc, WorldObject selected,
      Item casterItem)
    {
      SpellCast cast = ObtainPooledCast(caster);
      cast.TargetLoc = targetLoc;
      cast.SelectedTarget = selected;
      cast.CasterItem = casterItem;
      int num;
      cast.ExecuteInContext(() => num = (int) cast.Start(spell, true));
    }

    public void ExecuteInContext(Action action)
    {
      WorldObject casterObject = CasterObject;
      if(casterObject != null)
        casterObject.ExecuteInContext(action);
      else
        Map.ExecuteInContext(action);
    }

    private void InitializeClientCast(Spell spell)
    {
      Spell = spell;
      Map = CasterObject.Map;
      Phase = CasterObject.Phase;
      IsPlayerCast = true;
      IsCasting = true;
    }

    /// <summary>
    /// This starts a spell-cast, requested by the client.
    /// The client submits where or what the user selected in the packet.
    /// </summary>
    internal SpellFailedReason Start(Spell spell, Unit target)
    {
      if(IsCasting)
      {
        if(!IsChanneling)
        {
          SpellHandler.SendCastFailed(Client, spell,
            SpellFailedReason.SpellInProgress);
          return SpellFailedReason.SpellInProgress;
        }

        Cancel(SpellFailedReason.DontReport);
      }

      InitializeClientCast(spell);
      if(target == null)
        return SpellFailedReason.BadTargets;
      SelectedTarget = target;
      TargetLoc = SelectedTarget.Position;
      return Start();
    }

    private SpellFailedReason Start()
    {
      if(Spell.SpecialCast != null)
      {
        Spell.SpecialCast(Spell, CasterObject, SelectedTarget, ref m_targetLoc);
        Cancel(SpellFailedReason.DontReport);
        return SpellFailedReason.DontReport;
      }

      SpellFailedReason spellFailedReason1 = CheckSelectedTarget();
      if(spellFailedReason1 != SpellFailedReason.Ok)
        return spellFailedReason1;
      if(Spell.RequiredTargetId != 0U && SelectedTargetIsInvalid)
      {
        Cancel(SpellFailedReason.BadTargets);
        return SpellFailedReason.BadTargets;
      }

      if(Spell.Effect0_ImplicitTargetA == ImplicitSpellTargetType.AllParty)
      {
        if(CasterChar != null && CasterChar.Group != null)
          InitialTargets = CasterChar.Group.GetAllCharacters();
      }
      else
      {
        WorldObject[] worldObjectArray;
        if(!Spell.IsAreaSpell)
          worldObjectArray = new WorldObject[1]
          {
            SelectedTarget
          };
        else
          worldObjectArray = null;
        InitialTargets = worldObjectArray;
      }

      SpellFailedReason spellFailedReason2 = Prepare();
      if(spellFailedReason2 != SpellFailedReason.Ok)
        return spellFailedReason2;
      return FinishPrepare();
    }

    private bool SelectedTargetIsInvalid
    {
      get
      {
        if(SelectedTarget != null &&
           (int) SelectedTarget.EntryId == (int) Spell.RequiredTargetId)
          return !Spell.MatchesRequiredTargetType(SelectedTarget);
        return true;
      }
    }

    private SpellFailedReason CheckSelectedTarget()
    {
      if(SelectedTarget == null || SelectedTarget == CasterObject ||
         !(CasterObject is Character) || SelectedTarget.IsInWorld &&
         Utility.IsInRange(CasterObject.GetDistanceSq(ref m_targetLoc),
           (CasterObject as Character).GetSpellMaxRange(Spell, SelectedTarget)) &&
         (SelectedTarget == null || SelectedTarget.Map == CasterObject.Map))
        return SpellFailedReason.Ok;
      Cancel(SpellFailedReason.OutOfRange);
      return SpellFailedReason.OutOfRange;
    }

    private SpellFailedReason ReadTargetInfoFromPacket(RealmPacketIn packet)
    {
      TargetFlags = (SpellTargetFlags) packet.ReadUInt32();
      if(TargetFlags == SpellTargetFlags.Self)
      {
        SelectedTarget = CasterObject;
        TargetLoc = SelectedTarget.Position;
        return SpellFailedReason.Ok;
      }

      if(TargetFlags.HasAnyFlag(SpellTargetFlags.WorldObject))
      {
        SelectedTarget = Map.GetObject(packet.ReadPackedEntityId());
        if(SelectedTarget == null || !CasterObject.CanSee(SelectedTarget))
        {
          Cancel(SpellFailedReason.BadTargets);
          return SpellFailedReason.BadTargets;
        }

        TargetLoc = SelectedTarget.Position;
      }

      if(CasterObject is Character && TargetFlags.HasAnyFlag(SpellTargetFlags.AnyItem))
      {
        TargetItem = CasterChar.Inventory.GetItem(packet.ReadPackedEntityId());
        if(TargetItem == null || !TargetItem.CanBeUsed)
        {
          Cancel(SpellFailedReason.ItemGone);
          return SpellFailedReason.ItemGone;
        }
      }

      if(TargetFlags.HasAnyFlag(SpellTargetFlags.SourceLocation))
        Map.GetObject(packet.ReadPackedEntityId());
      SourceLoc = CasterObject.Position;
      if(TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
      {
        SelectedTarget = Map.GetObject(packet.ReadPackedEntityId());
        TargetLoc = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
      }

      if(TargetFlags.HasAnyFlag(SpellTargetFlags.String))
        StringTarget = packet.ReadCString();
      return SpellFailedReason.Ok;
    }

    private static void ReadUnknownDataFromPacket(PacketIn packet, byte unkFlags)
    {
      if((unkFlags & 2) == 0)
        return;
      double num1 = packet.ReadFloat();
      double num2 = packet.ReadFloat();
      int num3 = packet.ReadByte();
    }

    public SpellFailedReason Start(SpellId spell, bool passiveCast)
    {
      return Start(SpellHandler.Get(spell), passiveCast);
    }

    public SpellFailedReason Start(SpellId spell)
    {
      return Start(spell, false, WorldObject.EmptyArray);
    }

    public SpellFailedReason Start(Spell spell, bool passiveCast)
    {
      return Start(spell, (passiveCast ? 1 : 0) != 0, new WorldObject[1]
      {
        SelectedTarget
      });
    }

    public SpellFailedReason Start(SpellId spellId, bool passiveCast, params WorldObject[] targets)
    {
      Spell spell = SpellHandler.Get(spellId);
      if(spell != null)
        return Start(spell, passiveCast, targets);
      _log.Warn("{0} tried to cast non-existant Spell: {1}", CasterObject,
        spellId);
      return SpellFailedReason.DontReport;
    }

    public SpellFailedReason Start(Spell spell, bool passiveCast, WorldObject singleTarget)
    {
      WorldObject[] worldObjectArray = new WorldObject[1]
      {
        singleTarget
      };
      return Start(spell, passiveCast, worldObjectArray);
    }

    public SpellFailedReason Start(Spell spell, SpellEffect triggerEffect, bool passiveCast,
      params WorldObject[] initialTargets)
    {
      TriggerEffect = triggerEffect;
      return Start(spell, passiveCast, initialTargets);
    }

    public SpellFailedReason Start(Spell spell)
    {
      return Start(spell, false, WorldObject.EmptyArray);
    }

    public SpellFailedReason Start(Spell spell, bool passiveCast, params WorldObject[] initialTargets)
    {
      if(IsCasting || IsChanneling)
        Cancel(SpellFailedReason.Interrupted);
      IsCasting = true;
      Spell = spell;
      IsPassive = passiveCast;
      InitialTargets = initialTargets == null || initialTargets.Length == 0
        ? null
        : initialTargets;
      SpellFailedReason spellFailedReason = Prepare();
      if(spellFailedReason != SpellFailedReason.Ok)
        return spellFailedReason;
      return FinishPrepare();
    }

    /// <summary>
    /// Use this method to change the SpellCast object after it has been prepared.
    /// If no changes are necessary, simply use <see cref="M:WCell.RealmServer.Spells.SpellCast.Start(WCell.RealmServer.Spells.Spell,System.Boolean,WCell.RealmServer.Entities.WorldObject[])" />
    /// </summary>
    public SpellFailedReason Prepare(Spell spell, bool passiveCast, params WorldObject[] initialTargets)
    {
      if(IsCasting || IsChanneling)
        Cancel(SpellFailedReason.Interrupted);
      IsCasting = true;
      Spell = spell;
      IsPassive = passiveCast;
      InitialTargets = initialTargets;
      SpellFailedReason reason = Prepare();
      if(reason == SpellFailedReason.Ok)
      {
        reason = PrepareHandlers();
        if(reason != SpellFailedReason.Ok)
          Cancel(reason);
      }

      return reason;
    }

    private SpellFailedReason Prepare()
    {
      if(Spell == null)
      {
        _log.Warn("{0} tried to cast without selecting a Spell.", CasterObject);
        return SpellFailedReason.Error;
      }

      try
      {
        if(IsAICast)
        {
          SpellFailedReason reason = PrepareAI();
          if(reason != SpellFailedReason.Ok)
          {
            Cancel(reason);
            return reason;
          }
        }

        if(SelectedTarget == null && CasterUnit != null)
          SelectedTarget = InitialTargets == null
            ? CasterUnit.Target
            : InitialTargets[0];
        if(!IsPassive && !Spell.IsPassive && CasterUnit != null)
        {
          Spell spell = Spell;
          if(!GodMode && !IsPassive && CasterUnit.IsPlayer)
          {
            SpellFailedReason reason = CheckPlayerCast(SelectedTarget);
            if(reason != SpellFailedReason.Ok)
            {
              Cancel(reason);
              return reason;
            }
          }

          CasterUnit.Auras.RemoveByFlag(AuraInterruptFlags.OnCast);
        }

        StartTime = Environment.TickCount;
        m_castDelay = 0;
        if(CasterUnit != null && DateTime.Now.AddMilliseconds((int) Spell.CastDelay) >
           CasterUnit.CastingTill)
          CasterUnit.CastingTill = DateTime.Now.AddMilliseconds((int) Spell.CastDelay);
        if(!IsInstant && CasterUnit != null)
        {
          m_castDelay = MathUtil.RoundInt(CasterUnit.CastSpeedFactor * m_castDelay);
          m_castDelay =
            CasterUnit.Auras.GetModifiedInt(SpellModifierType.CastTime, Spell, m_castDelay);
        }

        if(Spell.TargetLocation != null)
          TargetLoc = Spell.TargetLocation.Position;
        return Spell.NotifyCasting(this);
      }
      catch(Exception ex)
      {
        OnException(ex);
        return SpellFailedReason.Error;
      }
    }

    private SpellFailedReason FinishPrepare()
    {
      try
      {
        if(!IsInstant)
        {
          if(CasterObject is Unit)
            ((Unit) CasterObject).SheathType = SheathType.None;
          m_castTimer.Start(m_castDelay);
          return SpellFailedReason.Ok;
        }

        if(!Spell.IsOnNextStrike)
          return Perform();
        if(!(CasterObject is Unit))
        {
          Cancel(SpellFailedReason.Interrupted);
          return SpellFailedReason.Error;
        }

        CasterUnit.SetSpellCast(this);
        return SpellFailedReason.Ok;
      }
      catch(Exception ex)
      {
        OnException(ex);
        return SpellFailedReason.Error;
      }
    }

    /// <summary>
    /// Is sent in either of 3 cases:
    /// 1. At the beginning of a Cast of a normal Spell that is not instant
    /// 2. After the last check if its instant and not a weapon ability
    /// 3. On Strike if its a weapon ability
    /// </summary>
    internal void SendCastStart()
    {
      if(!Spell.IsVisibleToClient)
        return;
      SpellHandler.SendCastStart(this);
    }

    internal void SendSpellGo(List<MissedTarget> missedTargets)
    {
      if(Spell.IsPassive || Spell.Attributes.HasAnyFlag(SpellAttributes.InvisibleAura) ||
         (Spell.HasEffectWith(effect =>
            effect.EffectType == SpellEffectType.OpenLock) || !Spell.IsVisibleToClient))
        return;
      byte previousRuneMask = UsesRunes ? CasterChar.PlayerSpells.Runes.GetActiveRuneMask() : (byte) 0;
      SpellHandler.SendSpellGo((IEntity) (CasterItem ?? (object) CasterReference), this,
        Targets, missedTargets, previousRuneMask);
    }

    /// <summary>Checks the current Cast when Players are using it</summary>
    protected SpellFailedReason CheckPlayerCast(WorldObject selected)
    {
      Character casterChar = CasterChar;
      if(!IsAoE && casterChar != selected && selected != null)
      {
        if(Spell.HasHarmfulEffects && selected is Unit &&
           (((Unit) selected).IsEvading || ((Unit) selected).IsInvulnerable))
          return SpellFailedReason.TargetAurastate;
      }
      else if(!casterChar.IsAlive)
        return SpellFailedReason.CasterDead;

      SpellFailedReason spellFailedReason = Spell.CheckCasterConstraints(casterChar);
      if(spellFailedReason != SpellFailedReason.Ok)
        return spellFailedReason;
      casterChar.CancelLooting();
      if(RequiredSkillIsTooLow(casterChar))
        return SpellFailedReason.MinSkill;
      if(Spell.IsTame)
      {
        NPC target = selected as NPC;
        if(target == null)
          return SpellFailedReason.BadTargets;
        if(target.CurrentTamer != null)
          return SpellFailedReason.AlreadyBeingTamed;
        if(CheckTame(casterChar, target) != TameFailReason.Ok)
          return SpellFailedReason.DontReport;
      }

      if(!casterChar.HasEnoughPowerToCast(Spell, null) ||
         UsesRunes && !casterChar.PlayerSpells.Runes.HasEnoughRunes(Spell))
        return SpellFailedReason.NoPower;
      return Spell.CheckItemRestrictions(TargetItem, casterChar.Inventory);
    }

    private bool RequiredSkillIsTooLow(Character caster)
    {
      if(Spell.Ability != null && Spell.Ability.RedValue > 0U)
        return caster.Skills.GetValue(Spell.Ability.Skill.Id) < Spell.Ability.RedValue;
      return false;
    }

    /// <summary>Check if SpellCast hit the targets.</summary>
    /// <remarks>Never returns null</remarks>
    private List<MissedTarget> CheckHit()
    {
      List<MissedTarget> missedTargetList = CastMissListPool.Obtain();
      if(GodMode || Spell.IsPassive || Spell.IsPhysicalAbility)
        return missedTargetList;
      hitChecker.Initialize(Spell, CasterObject);
      foreach(Unit target in UnitTargets.Where(target =>
        !Spell.IsBeneficialFor(CasterReference, target)))
      {
        CastMissReason reason = hitChecker.CheckHitAgainstTarget(target);
        if(reason != CastMissReason.None)
          missedTargetList.Add(new MissedTarget(target, reason));
      }

      return missedTargetList;
    }

    /// <summary>
    /// Checks the current SpellCast parameters for whether taming the Selected NPC is legal.
    /// Sends the TameFailure packet if it didn't work
    /// </summary>
    public static TameFailReason CheckTame(Character caster, NPC target)
    {
      TameFailReason reason;
      if(!target.IsAlive)
        reason = TameFailReason.TargetDead;
      else if(!target.Entry.IsTamable)
        reason = TameFailReason.NotTamable;
      else if(target.Entry.IsExoticPet && !caster.CanControlExoticPets)
        reason = TameFailReason.CantControlExotic;
      else if(target.HasMaster)
      {
        reason = TameFailReason.CreatureAlreadyOwned;
      }
      else
      {
        if(target.Level <= caster.Level)
          return TameFailReason.Ok;
        reason = TameFailReason.TooHighLevel;
      }

      if(caster != null)
        PetHandler.SendTameFailure(caster, reason);
      return reason;
    }

    /// <summary>Tries to consume the given amount of power</summary>
    public SpellFailedReason ConsumePower(int amount)
    {
      Unit casterUnit = CasterUnit;
      if(casterUnit != null)
      {
        if(Spell.PowerType != PowerType.Health)
        {
          if(!casterUnit.ConsumePower(Spell.Schools[0], Spell, amount))
            return SpellFailedReason.NoPower;
        }
        else
        {
          int health = casterUnit.Health;
          casterUnit.Health = health - amount;
          if(amount >= health)
            return SpellFailedReason.CasterDead;
        }
      }

      return SpellFailedReason.Ok;
    }

    public IAsda2Weapon GetWeapon()
    {
      if(!(CasterObject is Unit))
      {
        _log.Warn("{0} is not a Unit and casted Weapon Ability {1}", CasterObject,
          Spell);
        return null;
      }

      IAsda2Weapon weapon = ((Unit) CasterObject).GetWeapon(Spell.EquipmentSlot);
      if(weapon == null)
        _log.Warn("{0} casted {1} without required Weapon: {2}", CasterObject,
          Spell, Spell.EquipmentSlot);
      return weapon;
    }

    /// <summary>
    /// Tries to consume all reagents or cancels the cast if it failed
    /// </summary>
    public bool ConsumeReagents()
    {
      return true;
    }

    private void OnException(Exception e)
    {
      LogUtil.ErrorException(e, "{0} failed to cast Spell {1} (Targets: {2})", (object) CasterObject,
        (object) Spell, (object) Targets.ToString(", "));
      if(CasterObject != null && !CasterObject.IsPlayer)
        CasterObject.Delete();
      else if(Client != null)
        Client.Disconnect(false);
      if(!IsCasting)
        return;
      Cleanup();
    }

    public void Pushback(int millis)
    {
      if(IsChanneling)
        Channel.Pushback(millis);
      else
        RemainingCastTime += millis;
    }

    /// <summary>
    /// Caused by damage.
    /// Delays the cast and might result in interruption (only if not DoT).
    /// See: http://www.wowwiki.com/Interrupt
    /// </summary>
    public void Pushback()
    {
      if(GodMode || !IsCasting)
        return;
      if(Spell.InterruptFlags.HasFlag(InterruptFlags.OnTakeDamage))
      {
        Cancel(SpellFailedReason.Interrupted);
      }
      else
      {
        if(m_pushbacks >= 2 || RemainingCastTime <= 0)
          return;
        if(IsChanneling)
          Channel.Pushback(
            GetPushBackTime(Channel.Duration / ChannelPushbackFraction));
        else
          RemainingCastTime += GetPushBackTime(PushbackDelay);
        ++m_pushbacks;
      }
    }

    private int GetPushBackTime(int time)
    {
      if(CasterObject is Unit)
      {
        int spellInterruptProt = ((Unit) CasterObject).GetSpellInterruptProt(Spell);
        if(spellInterruptProt >= 100)
          return 0;
        time -= spellInterruptProt * time / 100;
        time = ((Unit) CasterObject).Auras.GetModifiedIntNegative(SpellModifierType.PushbackReduction,
          Spell, time);
      }

      return Math.Max(0, time);
    }

    public void Trigger(SpellId spell)
    {
      Trigger(SpellHandler.Get(spell));
    }

    public void TriggerSelf(SpellId spell)
    {
      TriggerSingle(SpellHandler.Get(spell), CasterObject);
    }

    public void TriggerSelf(Spell spell)
    {
      TriggerSingle(spell, CasterObject);
    }

    public void TriggerSingle(SpellId spell, WorldObject singleTarget)
    {
      TriggerSingle(SpellHandler.Get(spell), singleTarget);
    }

    /// <summary>
    /// Casts the given spell on the given target, inheriting this SpellCast's information.
    /// SpellCast will automatically be enqueued if the Character is currently not in the world.
    /// </summary>
    public void TriggerSingle(Spell spell, WorldObject singleTarget)
    {
      SpellCast cast = InheritSpellCast();
      int num;
      ExecuteInContext(() => num = (int) cast.Start(spell, true, singleTarget));
    }

    /// <summary>
    /// Triggers all given spells instantly on the given single target
    /// </summary>
    public void TriggerAll(WorldObject singleTarget, params Spell[] spells)
    {
      if(CasterObject is Character && !CasterObject.IsInWorld)
        CasterChar.AddMessage(
          new Message(() => TriggerAllSpells(singleTarget, spells)));
      else
        TriggerAllSpells(singleTarget, spells);
    }

    private void TriggerAllSpells(WorldObject singleTarget, params Spell[] spells)
    {
      SpellCast cast = SpellCastPool.Obtain();
      foreach(Spell spell in spells)
      {
        SetupInheritedCast(cast);
        int num = (int) cast.Start(spell, true, singleTarget);
      }
    }

    /// <summary>
    /// Casts the given spell on the given targets within this SpellCast's context.
    /// Finds targets automatically if the given targets are null.
    /// </summary>
    public void Trigger(SpellId spell, params WorldObject[] targets)
    {
      Trigger(spell, null, targets);
    }

    /// <summary>
    /// Casts the given spell on the given targets within this SpellCast's context.
    /// Finds targets automatically if the given targets are null.
    /// </summary>
    public void Trigger(SpellId spell, SpellEffect triggerEffect, params WorldObject[] targets)
    {
      Trigger(spell, triggerEffect, null, targets);
    }

    /// <summary>
    /// Casts the given spell on the given targets within this SpellCast's context.
    /// Finds targets automatically if the given targets are null.
    /// </summary>
    public void Trigger(SpellId spell, SpellEffect triggerEffect, IUnitAction triggerAction = null,
      params WorldObject[] targets)
    {
      Trigger(SpellHandler.Get(spell), triggerEffect, triggerAction, targets);
    }

    /// <summary>
    /// Casts the given spell on the given targets within this SpellCast's context.
    /// Determines targets and hostility, based on the given triggerEffect.
    /// </summary>
    public void Trigger(Spell spell, SpellEffect triggerEffect, params WorldObject[] initialTargets)
    {
      Trigger(spell, triggerEffect, null, initialTargets);
    }

    /// <summary>
    /// Casts the given spell on the given targets within this SpellCast's context.
    /// Determines targets and hostility, based on the given triggerEffect.
    /// </summary>
    public void Trigger(Spell spell, SpellEffect triggerEffect, IUnitAction triggerAction,
      params WorldObject[] initialTargets)
    {
      SpellCast cast = InheritSpellCast();
      cast.TriggerAction = triggerAction;
      int num;
      ExecuteInContext(() => num = (int) cast.Start(spell, triggerEffect, true, initialTargets));
    }

    /// <summary>
    /// Casts the given spell on the given targets within this SpellCast's context.
    /// Finds targets automatically if the given targets are null.
    /// </summary>
    public void Trigger(Spell spell, params WorldObject[] targets)
    {
      SpellCast cast = InheritSpellCast();
      int num;
      ExecuteInContext(() => num = (int) cast.Start(spell, true,
        targets == null || targets.Length <= 0 ? null : targets));
    }

    /// <summary>
    /// Casts the given spell on targets determined by the given Spell.
    /// The given selected object will be the target, if the spell is a single target spell.
    /// </summary>
    public void TriggerSelected(Spell spell, WorldObject selected)
    {
      SpellCast cast = InheritSpellCast();
      cast.SelectedTarget = selected;
      int num;
      ExecuteInContext(() => num = (int) cast.Start(spell, true));
    }

    private SpellCast InheritSpellCast()
    {
      SpellCast cast = SpellCastPool.Obtain();
      SetupInheritedCast(cast);
      return cast;
    }

    private void SetupInheritedCast(SpellCast cast)
    {
      cast.SetCaster(CasterReference, Map, Phase, SourceLoc);
      cast.TargetLoc = TargetLoc;
      cast.SelectedTarget = SelectedTarget;
      cast.CasterItem = CasterItem;
    }

    public static void ValidateAndTriggerNew(Spell spell, Unit caster, WorldObject target,
      SpellChannel usedChannel = null, Item usedItem = null, IUnitAction action = null,
      SpellEffect triggerEffect = null)
    {
      ValidateAndTriggerNew(spell, caster.SharedReference, caster, target, usedChannel, usedItem,
        action, triggerEffect);
    }

    public static void ValidateAndTriggerNew(Spell spell, ObjectReference caster, Unit triggerOwner,
      WorldObject target, SpellChannel usedChannel = null, Item usedItem = null, IUnitAction action = null,
      SpellEffect triggerEffect = null)
    {
      SpellCast spellCast = SpellCastPool.Obtain();
      spellCast.SetCaster(caster, target.Map, target.Phase, triggerOwner.Position);
      spellCast.SelectedTarget = target;
      spellCast.TargetLoc = usedChannel == null || usedChannel.Cast.CasterUnit != triggerOwner
        ? target.Position
        : triggerOwner.ChannelObject.Position;
      spellCast.TargetItem = spellCast.CasterItem = usedItem;
      spellCast.ValidateAndTrigger(spell, triggerOwner, target, action, triggerEffect);
    }

    /// <summary>
    /// Creates a new SpellCast object to trigger the given spell.
    /// Validates whether the given target is the correct target
    /// or if we have to look for the actual targets ourselves.
    /// Revalidate targets, if it is:
    /// - an area spell
    /// - a harmful spell and currently targeting friends
    /// - not harmful and targeting an enemy
    /// </summary>
    /// <param name="spell"></param>
    /// <param name="target"></param>
    public void ValidateAndTriggerNew(Spell spell, Unit triggerOwner, WorldObject target, IUnitAction action = null,
      SpellEffect triggerEffect = null)
    {
      InheritSpellCast().ValidateAndTrigger(spell, triggerOwner, target, action, triggerEffect);
    }

    public void ValidateAndTrigger(Spell spell, Unit triggerOwner, IUnitAction action = null)
    {
      if(action != null)
      {
        ++action.ReferenceCount;
        TriggerAction = action;
      }

      ValidateAndTrigger(spell, triggerOwner, null, action, null);
    }

    public void ValidateAndTrigger(Spell spell, Unit triggerOwner, WorldObject target, IUnitAction action = null,
      SpellEffect triggerEffect = null)
    {
      if(triggerOwner == null)
      {
        _log.Warn("triggerOwner is null when trying to proc spell: {0} (target: {1})", spell,
          target);
      }
      else
      {
        WorldObject[] worldObjectArray;
        if(spell.CasterIsTarget || !spell.HasTargets)
          worldObjectArray = new Unit[1]
          {
            triggerOwner
          };
        else if(target != null)
        {
          if(spell.IsAreaSpell || CasterObject == null ||
             spell.IsHarmfulFor(CasterReference, target) !=
             target.IsHostileWith(CasterObject))
            worldObjectArray = null;
          else
            worldObjectArray = new WorldObject[1] { target };
        }
        else
          worldObjectArray = null;

        if(action != null)
        {
          ++action.ReferenceCount;
          TriggerAction = action;
        }

        int num = (int) Start(spell, triggerEffect, true, worldObjectArray);
      }
    }

    public void Cancel(SpellFailedReason reason = SpellFailedReason.Interrupted)
    {
      if(!IsCasting)
        return;
      IsCasting = false;
      CloseChannel();
      Spell.NotifyCancelled(this, reason);
      if(reason != SpellFailedReason.Ok)
      {
        if(!IsPassive && Spell.IsVisibleToClient)
        {
          if(reason != SpellFailedReason.OutOfRange && CasterChar != null)
            CasterChar.SendSystemMessage(string.Format("Cast spell failed cause : {0}",
              reason));
        }
        else if(CasterObject != null && CasterObject.IsUsingSpell &&
                reason != SpellFailedReason.DontReport)
          CancelOriginalSpellCast(reason);
      }

      Cleanup();
    }

    private void CloseChannel()
    {
      if(Channel == null)
        return;
      Channel.Close(true);
    }

    private void CancelOriginalSpellCast(SpellFailedReason reason)
    {
      SpellCast spellCast = CasterObject.SpellCast;
      if(this == CasterObject.SpellCast)
        return;
      spellCast.Cancel(reason);
    }

    public void Update(int dt)
    {
      m_castTimer.Update(dt);
      if(!IsChanneling)
        return;
      Channel.Update(dt);
    }

    /// <summary>
    /// Close the timer and get rid of circular references; will be called automatically
    /// </summary>
    protected internal void Cleanup()
    {
      IsPlayerCast = false;
      IsCasting = false;
      if(Spell.IsTame && SelectedTarget is NPC)
        ((NPC) SelectedTarget).CurrentTamer = null;
      TargetItem = null;
      CasterItem = null;
      m_castTimer.Stop();
      InitialTargets = null;
      Handlers = null;
      IsPassive = false;
      m_pushbacks = 0;
      Spell = null;
      TriggerEffect = null;
      TargetFlags = SpellTargetFlags.Self;
      Targets.Clear();
      FinalCleanup();
    }

    private void FinalCleanup()
    {
      CleanupTriggerAction();
      CleanupHandlers();
      if(CasterObject != null && CasterObject.SpellCast == this)
        return;
      Dispose();
    }

    private void CleanupTriggerAction()
    {
      if(TriggerAction == null)
        return;
      --TriggerAction.ReferenceCount;
      TriggerAction = null;
    }

    private void CleanupHandlers()
    {
      if(Handlers == null)
        return;
      foreach(SpellEffectHandler spellEffectHandler in Handlers
        .Where(handler => handler != null))
        spellEffectHandler.Cleanup();
    }

    internal void Dispose()
    {
      if(CasterReference == null)
      {
        _log.Warn("Tried to dispose SpellCast twice: " + this);
      }
      else
      {
        Cancel(SpellFailedReason.Interrupted);
        if(Channel != null)
        {
          Channel.Dispose();
          Channel = null;
        }

        Targets.Clear();
        SourceLoc = Vector3.Zero;
        CasterObject = CasterUnit = null;
        CasterReference = null;
        Map = null;
        SelectedTarget = null;
        GodMode = false;
        SpellCastPool.Recycle(this);
      }
    }

    public void SendPacketToArea(RealmPacketOut packet)
    {
      if(CasterObject != null)
        CasterObject.SendPacketToArea(packet, true, false, Locale.Any, new float?());
      else
        Map.SendPacketToArea(packet, ref m_targetLoc, Phase);
    }

    /// <summary>
    /// Is called whenever the validy of the caster might have changed
    /// </summary>
    private void CheckCasterValidity()
    {
      if(CasterObject == null || CasterObject.IsInWorld && CasterObject.IsInContext)
        return;
      CasterObject = null;
      CasterUnit = null;
    }

    /// <summary>Remove target from targets set and handler targets</summary>
    /// <param name="target"></param>
    private void Remove(WorldObject target)
    {
      Targets.Remove(target);
      RemoveFromHandlerTargets(target);
    }

    private void RemoveFromHandlerTargets(WorldObject target)
    {
      foreach(SpellEffectHandler handler in Handlers)
        handler.m_targets.Remove(target);
    }

    private void RemoveFromHandlerTargets(List<MissedTarget> missedTargets)
    {
      foreach(MissedTarget missedTarget in missedTargets)
        RemoveFromHandlerTargets(missedTarget.Target);
    }

    public SpellEffectHandler GetHandler(SpellEffectType type)
    {
      if(Handlers == null)
        throw new InvalidOperationException("Tried to get Handler from unintialized SpellCast");
      return Handlers.FirstOrDefault(
        handler => handler.Effect.EffectType == type);
    }

    public override string ToString()
    {
      return Spell + " casted by " + CasterObject;
    }
  }
}