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
            ObjectPoolMgr.CreatePool<SpellCast>((Func<SpellCast>) (() => new SpellCast()), true);

        public static readonly ObjectPool<List<IAura>> AuraListPool =
            ObjectPoolMgr.CreatePool<List<IAura>>((Func<List<IAura>>) (() => new List<IAura>()), true);

        public static readonly ObjectPool<List<MissedTarget>> CastMissListPool =
            ObjectPoolMgr.CreatePool<List<MissedTarget>>((Func<List<MissedTarget>>) (() => new List<MissedTarget>(3)),
                true);

        public static readonly ObjectPool<List<SpellEffectHandler>> SpellEffectHandlerListPool =
            ObjectPoolMgr.CreatePool<List<SpellEffectHandler>>(
                (Func<List<SpellEffectHandler>>) (() => new List<SpellEffectHandler>(3)), true);

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
            Unit casterUnit = this.CasterUnit;
            this.SourceLoc = casterUnit.Position;
            if (casterUnit.Target != null)
                casterUnit.SpellCast.TargetLoc = casterUnit.Target.Position;
            return SpellFailedReason.Ok;
        }

        private bool PrePerformAI()
        {
            return true;
        }

        private void RevalidateAllTargets()
        {
            this.Targets.Clear();
            HashSet<SpellTargetCollection> targetCollectionSet = new HashSet<SpellTargetCollection>();
            foreach (SpellEffectHandler handler in this.Handlers)
                targetCollectionSet.Add(handler.Targets);
            foreach (SpellTargetCollection targetCollection in targetCollectionSet)
            {
                targetCollection.RevalidateAll();
                this.Targets.AddRange<WorldObject>((IEnumerable<WorldObject>) targetCollection);
            }
        }

        /// <summary>Called when finished casting</summary>
        private void OnAICasted()
        {
            if (this.Spell.AISettings.IdleTimeAfterCastMillis <= 0)
                return;
            this.CasterUnit.Idle(this.Spell.AISettings.IdleTimeAfterCastMillis);
        }

        /// <summary>
        /// Checks whether the given target resisted the debuff, represented through the given spell
        /// </summary>
        public static CastMissReason CheckDebuffResist(Unit target, Spell spell, int casterLevel, bool hostile)
        {
            CastMissReason castMissReason = CastMissReason.None;
            if (hostile && target.CheckDebuffResist(casterLevel, target.GetLeastResistantSchool(spell)))
                castMissReason = CastMissReason.Resist;
            return castMissReason;
        }

        private SpellFailedReason PrepAuras()
        {
            this.m_auraApplicationInfos = new List<AuraApplicationInfo>(4);
            SpellEffectHandler spellEffectHandler = (SpellEffectHandler) null;
            for (int index = 0; index < this.Handlers.Length; ++index)
            {
                SpellEffectHandler handler = this.Handlers[index];
                if (handler.Effect.IsAuraEffect && (spellEffectHandler == null ||
                                                    !spellEffectHandler.Effect.SharesTargetsWith(handler.Effect)))
                {
                    spellEffectHandler = handler;
                    if (handler.m_targets != null)
                    {
                        foreach (WorldObject target1 in (List<WorldObject>) handler.m_targets)
                        {
                            WorldObject target = target1;
                            if (target is Unit &&
                                !this.m_auraApplicationInfos.Any<AuraApplicationInfo>(
                                    (Func<AuraApplicationInfo, bool>) (info => info.Target == target)))
                            {
                                AuraIndexId auraUid = this.Spell.GetAuraUID(this.CasterReference, target);
                                SpellFailedReason err = SpellFailedReason.Ok;
                                if (((Unit) target).Auras.PrepareStackOrOverride(this.CasterReference, auraUid,
                                    this.Spell, ref err, this))
                                    this.m_auraApplicationInfos.Add(new AuraApplicationInfo((Unit) target));
                                else if (err != SpellFailedReason.Ok && !this.IsAoE)
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
            auras = SpellCast.AuraListPool.Obtain();
            bool persistsThroughDeath = this.Spell.PersistsThroughDeath;
            if (this.Spell.IsAreaAura)
            {
                if (dynObj != null || this.CasterObject != null &&
                    (persistsThroughDeath || !(this.CasterObject is Unit) || ((Unit) this.CasterObject).IsAlive))
                {
                    AreaAura areaAura = new AreaAura((WorldObject) dynObj ?? this.CasterObject, this.Spell);
                    if (dynObj != null)
                        auras.Add((IAura) areaAura);
                }
                else
                    LogManager.GetCurrentClassLogger().Warn(
                        "Tried to cast Spell {0} with invalid dynObj or Caster - dynObj: {1}, CasterObject: {2}, CasterUnit: {3}",
                        (object) this.Spell, (object) dynObj, (object) this.CasterObject, (object) this.CasterUnit);
            }

            for (int index = this.m_auraApplicationInfos.Count - 1; index >= 0; --index)
            {
                if (!this.Targets.Contains((WorldObject) this.m_auraApplicationInfos[index].Target))
                    this.m_auraApplicationInfos.RemoveAt(index);
            }

            if (this.m_auraApplicationInfos.Count == 0)
                return;
            for (int index = 0; index < this.Handlers.Length; ++index)
            {
                SpellEffectHandler handler = this.Handlers[index];
                if (handler is ApplyAuraEffectHandler)
                    ((ApplyAuraEffectHandler) handler).AddAuraHandlers(this.m_auraApplicationInfos);
            }

            if (missedTargets == null)
                missedTargets = SpellCast.CastMissListPool.Obtain();
            for (int index = 0; index < this.m_auraApplicationInfos.Count; ++index)
            {
                AuraApplicationInfo auraApplicationInfo = this.m_auraApplicationInfos[index];
                Unit target = auraApplicationInfo.Target;
                if (target.IsInContext && auraApplicationInfo.Handlers != null &&
                    (persistsThroughDeath || target.IsAlive))
                {
                    bool hostile = this.Spell.IsHarmfulFor(this.CasterReference, (WorldObject) target);
                    CastMissReason reason;
                    if (!this.IsPassive && !this.Spell.IsPreventionDebuff &&
                        (reason = SpellCast.CheckDebuffResist(target, this.Spell, this.CasterReference.Level,
                            hostile)) != CastMissReason.None)
                    {
                        missedTargets.Add(new MissedTarget((WorldObject) target, reason));
                    }
                    else
                    {
                        Aura aura = target.Auras.CreateAura(this.CasterReference, this.Spell,
                            auraApplicationInfo.Handlers, this.TargetItem, !this.Spell.IsPreventionDebuff && !hostile);
                        if (aura != null)
                        {
                            if (!this.Spell.IsPreventionDebuff &&
                                (this.Spell.AttributesExC & SpellAttributesExC.NoInitialAggro) ==
                                SpellAttributesExC.None && (hostile && target.IsInWorld) && target.IsAlive)
                            {
                                target.IsInCombat = true;
                                if (target is NPC && this.CasterUnit != null)
                                    ((NPC) target).ThreatCollection.AddNewIfNotExisted(this.CasterUnit);
                            }

                            auras.Add((IAura) aura);
                        }
                    }
                }
            }

            this.m_auraApplicationInfos = (List<AuraApplicationInfo>) null;
        }

        private SpellFailedReason PrepareHandlers()
        {
            SpellFailedReason failReason = SpellFailedReason.Ok;
            SpellEffectHandler[] handlers = this.CreateHandlers(ref failReason);
            if (failReason != SpellFailedReason.Ok)
                return failReason;
            this.Handlers = handlers;
            SpellFailedReason spellFailedReason = this.InitializeHandlers();
            if (spellFailedReason != SpellFailedReason.Ok)
                return spellFailedReason;
            return this.InitializeHandlersTargets();
        }

        private SpellEffectHandler[] CreateHandlers(ref SpellFailedReason failReason)
        {
            SpellEffectHandler[] handlers = new SpellEffectHandler[this.Spell.EffectHandlerCount];
            int h = 0;
            SpellTargetCollection targets = (SpellTargetCollection) null;
            foreach (SpellEffect effect in ((IEnumerable<SpellEffect>) this.Spell.Effects).Where<SpellEffect>(
                (Func<SpellEffect, bool>) (effect => effect.SpellEffectHandlerCreator != null)))
            {
                this.CreateHandler(effect, h, handlers, ref targets, ref failReason);
                if (failReason != SpellFailedReason.Ok)
                    return (SpellEffectHandler[]) null;
                ++h;
            }

            return handlers;
        }

        private void CreateHandler(SpellEffect effect, int h, SpellEffectHandler[] handlers,
            ref SpellTargetCollection targets, ref SpellFailedReason failReason)
        {
            SpellEffectHandler spellEffectHandler = effect.SpellEffectHandlerCreator(this, effect);
            handlers[h] = spellEffectHandler;
            spellEffectHandler.InitialTarget = (Unit) this.SelectedTarget;
            if (targets == null)
                targets = this.CreateSpellTargetCollection();
            if (targets == null)
                return;
            spellEffectHandler.m_targets = targets;
            targets.m_handlers.Add(spellEffectHandler);
        }

        private SpellFailedReason InitializeHandlers()
        {
            foreach (SpellEffectHandler handler in this.Handlers)
            {
                SpellFailedReason spellFailedReason = handler.Initialize();
                if (spellFailedReason != SpellFailedReason.Ok)
                {
                    this.Handlers = (SpellEffectHandler[]) null;
                    return spellFailedReason;
                }
            }

            return SpellFailedReason.Ok;
        }

        private SpellFailedReason InitializeHandlersTargets()
        {
            foreach (SpellEffectHandler handler in ((IEnumerable<SpellEffectHandler>) this.Handlers)
                .Where<SpellEffectHandler>((Func<SpellEffectHandler, bool>) (handler =>
                {
                    if (handler.Targets != null)
                        return !handler.Targets.IsInitialized;
                    return false;
                })))
            {
                SpellFailedReason spellFailedReason = this.CollectHandlerTargets(handler);
                if (spellFailedReason != SpellFailedReason.Ok)
                    return spellFailedReason;
            }

            return SpellFailedReason.Ok;
        }

        private SpellFailedReason CollectHandlerTargets(SpellEffectHandler handler)
        {
            SpellFailedReason spellFailedReason = this.InitialTargets != null
                ? handler.Targets.AddAll(this.InitialTargets)
                : handler.Targets.FindAllTargets();
            if (spellFailedReason != SpellFailedReason.Ok)
                return spellFailedReason;
            this.AddHandlerTargetsToTargets(handler);
            return SpellFailedReason.Ok;
        }

        private void AddHandlerTargetsToTargets(SpellEffectHandler handler)
        {
            foreach (WorldObject target in (List<WorldObject>) handler.Targets)
                this.Targets.Add(target);
        }

        private SpellTargetCollection CreateSpellTargetCollection()
        {
            return SpellTargetCollection.Obtain();
        }

        private void Perform(int elapsed)
        {
            this.CheckCasterValidity();
            int num = (int) this.Perform();
        }

        /// <summary>
        /// Does some sanity checks and adjustments right before perform
        /// </summary>
        protected SpellFailedReason PrePerform()
        {
            if (this.IsPlayerCast)
            {
                SpellFailedReason spellFailedReason = this.PlayerPrePerform();
                if (spellFailedReason != SpellFailedReason.Ok)
                    return spellFailedReason;
            }

            if (this.CasterUnit == null && this.Spell.IsChanneled)
                return SpellFailedReason.CasterAurastate;
            if (this.Spell.IsAura)
            {
                if (this.Targets.Count == 0 && !this.IsAoE && !this.Spell.IsAreaAura)
                    return SpellFailedReason.NoValidTargets;
                SpellFailedReason reason = this.PrepAuras();
                if (reason != SpellFailedReason.Ok)
                {
                    this.Cancel(reason);
                    return reason;
                }
            }

            if (this.CasterUnit != null)
            {
                if (this.SelectedLock != null && this.SelectedLock.RequiresKneeling)
                    this.CasterUnit.StandState = StandState.Kneeling;
                this.CancelStealth();
            }

            return this.IsAICast && !this.PrePerformAI() ? SpellFailedReason.NoValidTargets : SpellFailedReason.Ok;
        }

        private SpellFailedReason PlayerPrePerform()
        {
            if (this.Spell.TargetFlags.HasAnyFlag(SpellTargetFlags.Item))
            {
                SpellFailedReason spellFailedReason = this.CheckTargetItem();
                if (spellFailedReason != SpellFailedReason.Ok)
                    return spellFailedReason;
            }

            if (this.CastFailsDueToImmune)
            {
                this.Cancel(SpellFailedReason.Immune);
                return SpellFailedReason.Immune;
            }

            if (this.Spell.IsAutoRepeating)
                return this.ToggleAutorepeatingSpell();
            if (this.Spell.Attributes.HasFlag((Enum) SpellAttributes.StopsAutoAttack))
                this.StopAutoAttack();
            return SpellFailedReason.Ok;
        }

        private SpellFailedReason CheckTargetItem()
        {
            if (!this.ItemIsSelected)
            {
                this.LogWarnIfIsPassive("Trying to trigger Spell without Item selected: " + (object) this);
                return SpellFailedReason.ItemNotFound;
            }

            if (this.ItemIsReady)
                return SpellFailedReason.Ok;
            this.LogWarnIfIsPassive("Trying to trigger Spell without Item ready: " + (object) this);
            return SpellFailedReason.ItemNotReady;
        }

        private bool ItemIsSelected
        {
            get
            {
                if (this.TargetItem != null && this.TargetItem.IsInWorld)
                    return this.TargetItem.Owner == this.CasterObject;
                return false;
            }
        }

        private void LogWarnIfIsPassive(string message)
        {
            if (!this.IsPassive)
                return;
            SpellCast._log.Warn(message);
        }

        private bool ItemIsReady
        {
            get
            {
                if (this.TargetItem.IsEquipped)
                    return this.TargetItem.Unequip();
                return true;
            }
        }

        private bool CastFailsDueToImmune
        {
            get
            {
                if (this.CastCanFailDueToImmune)
                    return this.SelectedUnitIsImmuneToSpell;
                return false;
            }
        }

        private bool CastCanFailDueToImmune
        {
            get
            {
                if (!this.IsAoE && this.SelectedTarget is Unit)
                    return !this.Spell.IsPreventionDebuff;
                return false;
            }
        }

        private bool SelectedUnitIsImmuneToSpell
        {
            get
            {
                bool flag = this.Spell.IsHarmfulFor(this.CasterReference, this.SelectedTarget);
                Unit selectedTarget = (Unit) this.SelectedTarget;
                if (flag)
                    return selectedTarget.IsImmuneToSpell(this.Spell);
                return false;
            }
        }

        private SpellFailedReason ToggleAutorepeatingSpell()
        {
            if (this.CasterUnit.Target == null && !(this.SelectedTarget is Unit))
                return SpellFailedReason.BadTargets;
            this.CasterUnit.IsFighting = true;
            if (this.CasterUnit.AutorepeatSpell == this.Spell)
                this.DeactivateAutorepeatingSpell();
            else
                this.ActivateAutorepeatingSpell();
            return SpellFailedReason.DontReport;
        }

        private void ActivateAutorepeatingSpell()
        {
            this.CasterUnit.AutorepeatSpell = this.Spell;
            this.SendCastStart();
        }

        private void DeactivateAutorepeatingSpell()
        {
            this.CasterUnit.AutorepeatSpell = (Spell) null;
        }

        private void StopAutoAttack()
        {
            this.DeactivateAutorepeatingSpell();
            this.CasterUnit.IsFighting = false;
        }

        private void CancelStealth()
        {
            if (this.Spell.AttributesEx.HasFlag((Enum) SpellAttributesEx.RemainStealthed))
                return;
            this.CasterUnit.Auras.RemoveWhere((Predicate<Aura>) (aura => aura.Spell.DispelType == DispelType.Stealth));
        }

        private LockEntry SelectedLock
        {
            get { return (this.SelectedTarget as ILockable)?.Lock; }
        }

        /// <summary>Performs the actual Spell</summary>
        internal SpellFailedReason Perform()
        {
            try
            {
                if (this.Handlers == null)
                {
                    SpellFailedReason reason = this.PrepareHandlers();
                    if (reason != SpellFailedReason.Ok)
                    {
                        this.Cancel(reason);
                        return reason;
                    }
                }

                SpellFailedReason reason1 = this.PrePerform();
                if (reason1 != SpellFailedReason.Ok)
                {
                    this.Cancel(reason1);
                    return reason1;
                }

                SpellFailedReason spellFailedReason = this.Impact();
                if (this.IsCasting && this.CasterUnit != null)
                    this.OnUnitCasted();
                if (this.IsCasting && !this.IsChanneling)
                    this.Cleanup();
                return spellFailedReason;
            }
            catch (Exception ex)
            {
                this.OnException(ex);
                return SpellFailedReason.Error;
            }
        }

        /// <summary>
        /// Calculates the delay until a spell impacts its target in milliseconds
        /// </summary>
        /// <returns>delay in ms</returns>
        private int CalculateImpactDelay()
        {
            if (this.CasterChar != null)
                return 0;
            return (int) this.Spell.CastDelay;
        }

        private void DoDelayedImpact(int delay)
        {
            if (this.CasterObject != null)
            {
                this.CasterObject.CallDelayed(delay, new Action<WorldObject>(this.DelayedImpact));
                if (this.Spell.IsChanneled || this != this.CasterObject.SpellCast)
                    return;
                this.CasterObject.SpellCast = (SpellCast) null;
            }
            else
                this.Map.CallDelayed(delay, (Action) (() => this.DelayedImpact((WorldObject) null)));
        }

        private void DelayedImpact(WorldObject obj)
        {
            this.CheckCasterValidity();
            foreach (WorldObject target in this.Targets.Where<WorldObject>(
                (Func<WorldObject, bool>) (target => !target.IsInWorld)))
                this.Remove(target);
            try
            {
                int num = (int) this.Impact();
                if (!this.Spell.IsChanneled && this.IsCasting)
                    this.Cleanup();
                WorldObject casterObject = this.CasterObject;
                if (casterObject == null || casterObject.SpellCast != null || this.IsPassive)
                    return;
                casterObject.SpellCast = this;
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        /// <summary>Validates targets and applies all SpellEffects</summary>
        public SpellFailedReason Impact()
        {
            if (!this.IsCasting)
                return SpellFailedReason.Ok;
            foreach (SpellEffectHandler handler in this.Handlers)
            {
                if (!handler.Effect.IsPeriodic && !handler.Effect.IsStrikeEffect)
                {
                    handler.Apply();
                    if (!this.IsCasting)
                        return SpellFailedReason.DontReport;
                }
            }

            if (this.CasterObject is Unit && this.Spell.IsPhysicalAbility)
            {
                foreach (Unit unitTarget in this.UnitTargets)
                {
                    ProcHitFlags procHitFlags = this.CasterUnit.Strike(this.GetWeapon(), unitTarget, this);
                    this.m_hitInfoByTarget[unitTarget] = procHitFlags;
                }
            }

            DynamicObject dynObj = (DynamicObject) null;
            if (this.Spell.DOEffect != null)
                dynObj = new DynamicObject(this, this.Spell.DOEffect.GetRadius(this.CasterReference));
            if (!this.IsCasting)
                return SpellFailedReason.Ok;
            List<MissedTarget> missedTargets = (List<MissedTarget>) null;
            List<IAura> auras = (List<IAura>) null;
            if (this.m_auraApplicationInfos != null)
                this.CreateAuras(ref missedTargets, ref auras, dynObj);
            if (missedTargets != null)
            {
                if (missedTargets.Count > 0)
                {
                    CombatLogHandler.SendSpellMiss(this, true, (ICollection<MissedTarget>) missedTargets);
                    missedTargets.Clear();
                }

                SpellCast.CastMissListPool.Recycle(missedTargets);
            }

            if (this.Spell.IsChanneled && this.CasterObject != null)
            {
                this.Channel = SpellChannel.SpellChannelPool.Obtain();
                this.Channel.m_cast = this;
                if (this.CasterObject is Unit)
                {
                    if (dynObj != null)
                        this.CasterUnit.ChannelObject = (WorldObject) dynObj;
                    else if (this.SelectedTarget != null)
                    {
                        this.CasterUnit.ChannelObject = this.SelectedTarget;
                        if (this.SelectedTarget is NPC && this.Spell.IsTame)
                            ((NPC) this.SelectedTarget).CurrentTamer = this.CasterObject as Character;
                    }
                }

                int length = this.Handlers.Length;
                List<SpellEffectHandler> channelHandlers = SpellCast.SpellEffectHandlerListPool.Obtain();
                for (int index = 0; index < length; ++index)
                {
                    SpellEffectHandler handler = this.Handlers[index];
                    if (handler.Effect.IsPeriodic)
                        channelHandlers.Add(handler);
                }

                this.Channel.Open(channelHandlers, auras);
            }

            if (auras != null)
            {
                for (int index = 0; index < auras.Count; ++index)
                    auras[index].Start(this.Spell.IsChanneled ? (ITickTimer) this.Channel : (ITickTimer) null, false);
                if (!this.IsChanneling)
                {
                    auras.Clear();
                    SpellCast.AuraListPool.Recycle(auras);
                    auras = (List<IAura>) null;
                }
            }

            if (this.Spell.HasHarmfulEffects && !this.Spell.IsPreventionDebuff)
            {
                foreach (WorldObject target in this.Targets)
                {
                    if (target is Unit && this.Spell.IsHarmfulFor(this.CasterReference, target))
                        ((Unit) target).Auras.RemoveByFlag(AuraInterruptFlags.OnHostileSpellInflicted);
                }
            }

            return SpellFailedReason.Ok;
        }

        protected void OnUnitCasted()
        {
            this.OnAliveUnitCasted();
            this.OnTargetItemUsed();
            this.UpdateAuraState();
            if (!this.GodMode)
            {
                this.OnNonGodModeSpellCasted();
                if (!this.IsCasting)
                    return;
            }
            else if (!this.IsPassive && this.CasterUnit is Character)
                this.ClearCooldowns();

            this.AddRunicPower();
            this.TriggerSpellsAfterCastingSpells();
            if (!this.IsCasting)
                return;
            this.TriggerDynamicPostCastSpells();
            if (!this.IsCasting)
                return;
            this.ConsumeCombopoints();
            this.ConsumeSpellModifiers();
            if (!this.IsCasting)
                return;
            if (this.IsAICast)
            {
                this.OnAICasted();
                if (!this.IsCasting)
                    return;
            }

            this.Spell.NotifyCasted(this);
            if (this.CasterUnit is Character)
                this.CasterChar.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CastSpell,
                    this.Spell.Id, 0U, (Unit) null);
            this.TriggerProcOnCasted();
            this.m_hitInfoByTarget.Clear();
        }

        private void OnAliveUnitCasted()
        {
            if (!this.CasterUnit.IsAlive)
                return;
            if (this.CasterUnit is Character)
                this.OnAliveCharacterCasted();
            this.PutCasterInCombatModeAfterCastOnCombatant();
            this.ResetSwingDelay();
        }

        private void OnAliveCharacterCasted()
        {
            this.SitWhileConsuming();
            this.GainSkill();
            if (this.UsedCombatAbility)
                this.OnCharacterCombatAbilityUsed();
            this.CheckForQuestProgress();
        }

        private void SitWhileConsuming()
        {
            if (!this.Spell.IsFood && !this.Spell.IsDrink)
                return;
            this.CasterChar.StandState = StandState.Sit;
            if (!this.Spell.IsFood)
                return;
            this.CasterChar.Emote(EmoteType.SimpleEat);
        }

        private void GainSkill()
        {
            if (this.Spell.Ability == null || !this.Spell.Ability.CanGainSkill)
                return;
            Skill skill = this.CasterChar.Skills[this.Spell.Ability.Skill.Id];
            ushort currentValue = skill.CurrentValue;
            ushort actualMax = (ushort) skill.ActualMax;
            if ((int) currentValue >= (int) actualMax)
                return;
            ushort num = (ushort) ((uint) currentValue + (uint) (ushort) this.Spell.Ability.Gain((int) currentValue));
            skill.CurrentValue = (int) num <= (int) actualMax ? num : actualMax;
        }

        private bool UsedCombatAbility
        {
            get
            {
                if (this.Spell.IsPhysicalAbility)
                    return this.Spell.IsRangedAbility;
                return false;
            }
        }

        private void OnCharacterCombatAbilityUsed()
        {
        }

        private void CheckForQuestProgress()
        {
            this.CasterChar.QuestLog.OnSpellCast(this);
        }

        private void PutCasterInCombatModeAfterCastOnCombatant()
        {
            if (this.CasterUnit.IsInCombat ||
                this.UnitTargets.Where<Unit>((Func<Unit, bool>) (target => target.IsInCombat)).Count<Unit>() <= 0)
                return;
            this.CasterUnit.IsInCombat = true;
        }

        private void ResetSwingDelay()
        {
            if (!this.Spell.HasHarmfulEffects || this.Spell.IsPreventionDebuff || !this.CasterUnit.IsInCombat)
                return;
            this.CasterUnit.ResetSwingDelay();
        }

        private void OnTargetItemUsed()
        {
            if (this.TargetItem == null)
                return;
            this.CasterChar.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.UseItem, this.Spell.Id,
                0U, (Unit) null);
            this.TargetItem.OnUse();
        }

        private void ConsumeSpellModifiers()
        {
            this.CasterUnit.Auras.OnCasted(this);
        }

        private void TriggerDynamicPostCastSpells()
        {
            this.CasterUnit.Spells.TriggerSpellsFor(this);
        }

        private void ConsumeCombopoints()
        {
            if (!this.Spell.IsFinishingMove)
                return;
            this.CasterUnit.ModComboState((Unit) null, 0);
        }

        private void TriggerSpellsAfterCastingSpells()
        {
            this.TriggerTargetTriggerSpells();
            if (!this.IsCasting)
                return;
            this.TriggerCasterTriggerSpells();
        }

        private void TriggerCasterTriggerSpells()
        {
            if (this.Spell.CasterTriggerSpells == null)
                return;
            foreach (Spell casterTriggerSpell in this.Spell.CasterTriggerSpells)
            {
                this.Trigger(casterTriggerSpell, this.Targets.ToArray<WorldObject>());
                if (!this.IsCasting)
                    break;
            }
        }

        private void TriggerTargetTriggerSpells()
        {
            if (this.Spell.TargetTriggerSpells == null)
                return;
            foreach (Spell targetTriggerSpell in this.Spell.TargetTriggerSpells)
            {
                this.Trigger(targetTriggerSpell, this.Targets.ToArray<WorldObject>());
                if (!this.IsCasting)
                    break;
            }
        }

        private void AddRunicPower()
        {
            if (!this.UsesRunes)
                return;
            this.CasterUnit.Power += this.Spell.RuneCostEntry.RunicPowerGain;
        }

        private void OnNonGodModeSpellCasted()
        {
            this.AddCooldown();
            this.ConsumeRunes();
            this.ConsumePower();
        }

        private void ClearCooldowns()
        {
            PlayerSpellCollection playerSpells = this.CasterChar.PlayerSpells;
            if (playerSpells == null)
                return;
            playerSpells.ClearCooldown(this.Spell, true);
        }

        private void ConsumePower()
        {
            int num = this.Spell.CalcPowerCost(this.CasterUnit,
                this.SelectedTarget is Unit
                    ? ((Unit) this.SelectedTarget).GetLeastResistantSchool(this.Spell)
                    : this.Spell.Schools[0]);
            if (this.Spell.PowerType != PowerType.Health)
                this.CasterUnit.Power -= num;
            else
                this.CasterUnit.Health -= num;
        }

        private void ConsumeRunes()
        {
            if (!this.UsesRunes)
                return;
            this.CasterChar.PlayerSpells.Runes.ConsumeRunes(this.Spell);
        }

        private void AddCooldown()
        {
            if (!this.Spell.IsAutoRepeating && this.TriggerEffect == null)
                this.CasterUnit.Spells.AddCooldown(this.Spell, this.CasterItem);
            if (this.Client == null ||
                this.Spell.Attributes.HasFlag((Enum) SpellAttributes.StartCooldownAfterEffectFade) ||
                this.CasterItem == null)
                return;
            SpellHandler.SendItemCooldown(this.Client, this.Spell.Id, (IEntity) this.CasterItem);
        }

        private void UpdateAuraState()
        {
            if (this.Spell.RequiredCasterAuraState != AuraState.DodgeOrBlockOrParry)
                return;
            this.CasterUnit.AuraState &= ~AuraStateMask.DodgeOrBlockOrParry;
        }

        private void TriggerProcOnCasted()
        {
            ProcTriggerFlags flags1 = ProcTriggerFlags.None;
            ProcTriggerFlags flags2 = ProcTriggerFlags.None;
            switch (this.Spell.DamageType)
            {
                case DamageType.None:
                    if (this.Spell.IsBeneficial)
                    {
                        flags1 |= ProcTriggerFlags.DoneBeneficialSpell;
                        flags2 |= ProcTriggerFlags.ReceivedBeneficialSpell;
                        break;
                    }

                    if (this.Spell.IsHarmful)
                    {
                        flags1 |= ProcTriggerFlags.DoneHarmfulSpell;
                        flags2 |= ProcTriggerFlags.ReceivedHarmfulSpell;
                        break;
                    }

                    break;
                case DamageType.Magic:
                    if (this.Spell.IsBeneficial)
                    {
                        flags1 |= ProcTriggerFlags.DoneBeneficialMagicSpell;
                        flags2 |= ProcTriggerFlags.ReceivedBeneficialMagicSpell;
                        break;
                    }

                    if (this.Spell.IsHarmful)
                    {
                        flags1 |= ProcTriggerFlags.DoneHarmfulMagicSpell;
                        flags2 |= ProcTriggerFlags.ReceivedHarmfulMagicSpell;
                        break;
                    }

                    break;
                case DamageType.Melee:
                    flags1 |= ProcTriggerFlags.DoneMeleeSpell;
                    flags2 |= ProcTriggerFlags.ReceivedMeleeSpell;
                    break;
                case DamageType.Ranged:
                    if (this.Spell.IsAutoRepeating)
                    {
                        flags1 |= ProcTriggerFlags.DoneRangedAutoAttack;
                        flags2 |= ProcTriggerFlags.ReceivedRangedAutoAttack;
                        break;
                    }

                    flags1 |= ProcTriggerFlags.DoneRangedSpell;
                    flags2 |= ProcTriggerFlags.ReceivedRangedSpell;
                    break;
            }

            ProcHitFlags hitFlags = this.TriggerProcOnTargets(flags2);
            this.TriggerProcOnCaster(flags1, hitFlags);
        }

        /// <summary>Triggers proc on all targets of SpellCast</summary>
        /// <param name="flags">What happened to targets ie. ProcTriggerFlags.ReceivedHarmfulSpell</param>
        /// <returns>Combination of hit result on all targets.</returns>
        private ProcHitFlags TriggerProcOnTargets(ProcTriggerFlags flags)
        {
            ProcHitFlags procHitFlags1 = ProcHitFlags.None;
            foreach (KeyValuePair<Unit, ProcHitFlags> keyValuePair in this.m_hitInfoByTarget)
            {
                Unit key = keyValuePair.Key;
                ProcHitFlags procHitFlags2 = keyValuePair.Value;
                procHitFlags1 |= procHitFlags2;
                SimpleUnitAction simpleUnitAction = new SimpleUnitAction()
                {
                    Attacker = this.CasterUnit,
                    Spell = this.Spell,
                    Victim = key,
                    IsCritical = procHitFlags2.HasAnyFlag(ProcHitFlags.CriticalHit)
                };
                key.Proc(flags, this.CasterUnit, (IUnitAction) simpleUnitAction, true, procHitFlags2);
            }

            return procHitFlags1;
        }

        /// <summary>Trigger proc on the caster of the spell.</summary>
        /// <param name="flags">What spell caster casted ie. ProcTriggerFlags.DoneHarmfulSpell</param>
        /// <param name="hitFlags">Hit result of the spell</param>
        private void TriggerProcOnCaster(ProcTriggerFlags flags, ProcHitFlags hitFlags)
        {
            SimpleUnitAction simpleUnitAction = new SimpleUnitAction()
            {
                Attacker = this.CasterUnit,
                Spell = this.Spell,
                Victim = this.m_hitInfoByTarget.Count > 0
                    ? this.m_hitInfoByTarget.First<KeyValuePair<Unit, ProcHitFlags>>().Key
                    : (Unit) null,
                IsCritical = hitFlags.HasAnyFlag(ProcHitFlags.CriticalHit)
            };
            Unit triggerer = this.UnitTargets.FirstOrDefault<Unit>();
            this.CasterUnit.Proc(flags, triggerer, (IUnitAction) simpleUnitAction, true, hitFlags);
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
            get { return this.Targets.OfType<Unit>(); }
        }

        public SpellTargetFlags TargetFlags { get; set; }

        public Map TargetMap
        {
            get { return this.Spell.TargetLocation == null ? this.Map : this.Spell.TargetLocation.Map ?? this.Map; }
        }

        /// <summary>
        /// The target location for a spell which has been sent by the player
        /// </summary>
        public Vector3 TargetLoc
        {
            get { return this.m_targetLoc; }
            set { this.m_targetLoc = value; }
        }

        public float TargetOrientation
        {
            get
            {
                if (this.Spell.TargetLocation != null || this.CasterObject == null)
                    return this.Spell.TargetOrientation;
                return this.CasterObject.Orientation;
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
            get { return this.CasterUnit as Character; }
        }

        /// <summary>
        /// This corresponds to the actual level of Units
        /// and for GOs returns the level of the owner.
        /// </summary>
        public int CasterLevel
        {
            get { return this.CasterReference.Level; }
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
            get { return (this.CasterUnit as Character)?.Client; }
        }

        /// <summary>The map where the SpellCast happens</summary>
        public Map Map { get; internal set; }

        /// <summary>Needed for IWorldLocation interface</summary>
        public MapId MapId
        {
            get { return this.Map.MapId; }
        }

        /// <summary>Needed for IWorldLocation interface</summary>
        public Vector3 Position
        {
            get { return this.SourceLoc; }
        }

        /// <summary>The context to which the SpellCast belongs</summary>
        public IContextHandler Context
        {
            get { return (IContextHandler) this.Map; }
        }

        public uint Phase { get; internal set; }

        public CastFlags StartFlags
        {
            get
            {
                CastFlags castFlags = CastFlags.None;
                if (this.Spell != null)
                {
                    if (this.Spell.IsRangedAbility)
                        castFlags |= CastFlags.Ranged;
                    if (this.UsesRunes)
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
                if (this.Spell.IsRangedAbility)
                    castFlags |= CastFlags.Ranged;
                if (this.UsesRunes)
                {
                    castFlags |= CastFlags.RuneAbility;
                    if (this.Spell.RuneCostEntry.RunicPowerGain > 0)
                        castFlags |= CastFlags.RunicPowerGain;
                    if (this.Spell.RuneCostEntry.CostsRunes)
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
                if (!this.GodMode)
                    return this.m_castDelay;
                return 1;
            }
        }

        /// <summary>
        /// The time in milliseconds between now and the actual casting (meaningless if smaller equal 0).
        /// Can be changed. Might return bogus numbers if not casting.
        /// </summary>
        public int RemainingCastTime
        {
            get { return this.CastDelay + this.StartTime - Environment.TickCount; }
            set
            {
                int delay = Math.Max(0, value - this.RemainingCastTime);
                this.StartTime = Environment.TickCount + delay;
                this.m_castTimer.RemainingInitialDelayMillis = value;
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
                if (this.IsPlayerCast || this.IsPassive)
                    return false;
                if (this.CasterUnit != null)
                    return !this.CasterUnit.IsPlayer;
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
                if (this.Channel != null)
                    return this.Channel.IsChanneling;
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
                if (this.IsCasting)
                    return this.Spell.IsOnNextStrike;
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
                if (!this.IsPassive && !this.GodMode)
                    return this.m_castDelay < 100;
                return true;
            }
        }

        public bool IsAoE
        {
            get
            {
                if (this.TriggerEffect == null)
                    return this.Spell.IsAreaSpell;
                return this.TriggerEffect.IsAreaEffect;
            }
        }

        public bool UsesRunes
        {
            get
            {
                if (this.Spell.RuneCostEntry != null && this.CasterChar != null)
                    return this.CasterChar.PlayerSpells.Runes != null;
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
            this.m_castTimer = new TimerEntry(new Action<int>(this.Perform));
            this.Targets = new HashSet<WorldObject>();
        }

        public static SpellCast ObtainPooledCast(WorldObject caster)
        {
            SpellCast spellCast = SpellCast.SpellCastPool.Obtain();
            spellCast.SetCaster(caster);
            return spellCast;
        }

        private void SetCaster(WorldObject caster)
        {
            this.CasterReference = caster.SharedReference;
            this.CasterObject = caster;
            this.CasterUnit = caster.UnitMaster;
            this.Map = caster.Map;
            this.Phase = caster.Phase;
        }

        private void SetCaster(ObjectReference caster, Map map, uint phase, Vector3 sourceLoc)
        {
            this.CasterReference = caster;
            if (caster == null)
                throw new ArgumentNullException(nameof(caster));
            this.CasterObject = caster.Object;
            this.CasterUnit = caster.UnitMaster;
            this.Map = map;
            this.Phase = phase;
            this.SourceLoc = sourceLoc;
        }

        public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc)
        {
            SpellCast.Trigger(caster, spell, ref targetLoc, (WorldObject) null);
        }

        public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc, WorldObject selected)
        {
            SpellCast.Trigger(caster, spell, ref targetLoc, selected, (Item) null);
        }

        public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc, WorldObject selected,
            Item casterItem)
        {
            SpellCast cast = SpellCast.ObtainPooledCast(caster);
            cast.TargetLoc = targetLoc;
            cast.SelectedTarget = selected;
            cast.CasterItem = casterItem;
            int num;
            cast.ExecuteInContext((Action) (() => num = (int) cast.Start(spell, true)));
        }

        public void ExecuteInContext(Action action)
        {
            WorldObject casterObject = this.CasterObject;
            if (casterObject != null)
                casterObject.ExecuteInContext(action);
            else
                this.Map.ExecuteInContext(action);
        }

        private void InitializeClientCast(Spell spell)
        {
            this.Spell = spell;
            this.Map = this.CasterObject.Map;
            this.Phase = this.CasterObject.Phase;
            this.IsPlayerCast = true;
            this.IsCasting = true;
        }

        /// <summary>
        /// This starts a spell-cast, requested by the client.
        /// The client submits where or what the user selected in the packet.
        /// </summary>
        internal SpellFailedReason Start(Spell spell, Unit target)
        {
            if (this.IsCasting)
            {
                if (!this.IsChanneling)
                {
                    SpellHandler.SendCastFailed((IPacketReceiver) this.Client, spell,
                        SpellFailedReason.SpellInProgress);
                    return SpellFailedReason.SpellInProgress;
                }

                this.Cancel(SpellFailedReason.DontReport);
            }

            this.InitializeClientCast(spell);
            if (target == null)
                return SpellFailedReason.BadTargets;
            this.SelectedTarget = (WorldObject) target;
            this.TargetLoc = this.SelectedTarget.Position;
            return this.Start();
        }

        private SpellFailedReason Start()
        {
            if (this.Spell.SpecialCast != null)
            {
                this.Spell.SpecialCast(this.Spell, this.CasterObject, this.SelectedTarget, ref this.m_targetLoc);
                this.Cancel(SpellFailedReason.DontReport);
                return SpellFailedReason.DontReport;
            }

            SpellFailedReason spellFailedReason1 = this.CheckSelectedTarget();
            if (spellFailedReason1 != SpellFailedReason.Ok)
                return spellFailedReason1;
            if (this.Spell.RequiredTargetId != 0U && this.SelectedTargetIsInvalid)
            {
                this.Cancel(SpellFailedReason.BadTargets);
                return SpellFailedReason.BadTargets;
            }

            if (this.Spell.Effect0_ImplicitTargetA == ImplicitSpellTargetType.AllParty)
            {
                if (this.CasterChar != null && this.CasterChar.Group != null)
                    this.InitialTargets = (WorldObject[]) this.CasterChar.Group.GetAllCharacters();
            }
            else
            {
                WorldObject[] worldObjectArray;
                if (!this.Spell.IsAreaSpell)
                    worldObjectArray = new WorldObject[1]
                    {
                        this.SelectedTarget
                    };
                else
                    worldObjectArray = (WorldObject[]) null;
                this.InitialTargets = worldObjectArray;
            }

            SpellFailedReason spellFailedReason2 = this.Prepare();
            if (spellFailedReason2 != SpellFailedReason.Ok)
                return spellFailedReason2;
            return this.FinishPrepare();
        }

        private bool SelectedTargetIsInvalid
        {
            get
            {
                if (this.SelectedTarget != null &&
                    (int) this.SelectedTarget.EntryId == (int) this.Spell.RequiredTargetId)
                    return !this.Spell.MatchesRequiredTargetType(this.SelectedTarget);
                return true;
            }
        }

        private SpellFailedReason CheckSelectedTarget()
        {
            if (this.SelectedTarget == null || this.SelectedTarget == this.CasterObject ||
                !(this.CasterObject is Character) || this.SelectedTarget.IsInWorld &&
                Utility.IsInRange(this.CasterObject.GetDistanceSq(ref this.m_targetLoc),
                    (this.CasterObject as Character).GetSpellMaxRange(this.Spell, this.SelectedTarget)) &&
                (this.SelectedTarget == null || this.SelectedTarget.Map == this.CasterObject.Map))
                return SpellFailedReason.Ok;
            this.Cancel(SpellFailedReason.OutOfRange);
            return SpellFailedReason.OutOfRange;
        }

        private SpellFailedReason ReadTargetInfoFromPacket(RealmPacketIn packet)
        {
            this.TargetFlags = (SpellTargetFlags) packet.ReadUInt32();
            if (this.TargetFlags == SpellTargetFlags.Self)
            {
                this.SelectedTarget = this.CasterObject;
                this.TargetLoc = this.SelectedTarget.Position;
                return SpellFailedReason.Ok;
            }

            if (this.TargetFlags.HasAnyFlag(SpellTargetFlags.WorldObject))
            {
                this.SelectedTarget = this.Map.GetObject(packet.ReadPackedEntityId());
                if (this.SelectedTarget == null || !this.CasterObject.CanSee(this.SelectedTarget))
                {
                    this.Cancel(SpellFailedReason.BadTargets);
                    return SpellFailedReason.BadTargets;
                }

                this.TargetLoc = this.SelectedTarget.Position;
            }

            if (this.CasterObject is Character && this.TargetFlags.HasAnyFlag(SpellTargetFlags.AnyItem))
            {
                this.TargetItem = this.CasterChar.Inventory.GetItem(packet.ReadPackedEntityId());
                if (this.TargetItem == null || !this.TargetItem.CanBeUsed)
                {
                    this.Cancel(SpellFailedReason.ItemGone);
                    return SpellFailedReason.ItemGone;
                }
            }

            if (this.TargetFlags.HasAnyFlag(SpellTargetFlags.SourceLocation))
                this.Map.GetObject(packet.ReadPackedEntityId());
            this.SourceLoc = this.CasterObject.Position;
            if (this.TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
            {
                this.SelectedTarget = this.Map.GetObject(packet.ReadPackedEntityId());
                this.TargetLoc = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
            }

            if (this.TargetFlags.HasAnyFlag(SpellTargetFlags.String))
                this.StringTarget = packet.ReadCString();
            return SpellFailedReason.Ok;
        }

        private static void ReadUnknownDataFromPacket(PacketIn packet, byte unkFlags)
        {
            if (((int) unkFlags & 2) == 0)
                return;
            double num1 = (double) packet.ReadFloat();
            double num2 = (double) packet.ReadFloat();
            int num3 = (int) packet.ReadByte();
        }

        public SpellFailedReason Start(SpellId spell, bool passiveCast)
        {
            return this.Start(SpellHandler.Get(spell), passiveCast);
        }

        public SpellFailedReason Start(SpellId spell)
        {
            return this.Start(spell, false, WorldObject.EmptyArray);
        }

        public SpellFailedReason Start(Spell spell, bool passiveCast)
        {
            return this.Start(spell, (passiveCast ? 1 : 0) != 0, new WorldObject[1]
            {
                this.SelectedTarget
            });
        }

        public SpellFailedReason Start(SpellId spellId, bool passiveCast, params WorldObject[] targets)
        {
            Spell spell = SpellHandler.Get(spellId);
            if (spell != null)
                return this.Start(spell, passiveCast, targets);
            SpellCast._log.Warn("{0} tried to cast non-existant Spell: {1}", (object) this.CasterObject,
                (object) spellId);
            return SpellFailedReason.DontReport;
        }

        public SpellFailedReason Start(Spell spell, bool passiveCast, WorldObject singleTarget)
        {
            WorldObject[] worldObjectArray = new WorldObject[1]
            {
                singleTarget
            };
            return this.Start(spell, passiveCast, worldObjectArray);
        }

        public SpellFailedReason Start(Spell spell, SpellEffect triggerEffect, bool passiveCast,
            params WorldObject[] initialTargets)
        {
            this.TriggerEffect = triggerEffect;
            return this.Start(spell, passiveCast, initialTargets);
        }

        public SpellFailedReason Start(Spell spell)
        {
            return this.Start(spell, false, WorldObject.EmptyArray);
        }

        public SpellFailedReason Start(Spell spell, bool passiveCast, params WorldObject[] initialTargets)
        {
            if (this.IsCasting || this.IsChanneling)
                this.Cancel(SpellFailedReason.Interrupted);
            this.IsCasting = true;
            this.Spell = spell;
            this.IsPassive = passiveCast;
            this.InitialTargets = initialTargets == null || initialTargets.Length == 0
                ? (WorldObject[]) null
                : initialTargets;
            SpellFailedReason spellFailedReason = this.Prepare();
            if (spellFailedReason != SpellFailedReason.Ok)
                return spellFailedReason;
            return this.FinishPrepare();
        }

        /// <summary>
        /// Use this method to change the SpellCast object after it has been prepared.
        /// If no changes are necessary, simply use <see cref="M:WCell.RealmServer.Spells.SpellCast.Start(WCell.RealmServer.Spells.Spell,System.Boolean,WCell.RealmServer.Entities.WorldObject[])" />
        /// </summary>
        public SpellFailedReason Prepare(Spell spell, bool passiveCast, params WorldObject[] initialTargets)
        {
            if (this.IsCasting || this.IsChanneling)
                this.Cancel(SpellFailedReason.Interrupted);
            this.IsCasting = true;
            this.Spell = spell;
            this.IsPassive = passiveCast;
            this.InitialTargets = initialTargets;
            SpellFailedReason reason = this.Prepare();
            if (reason == SpellFailedReason.Ok)
            {
                reason = this.PrepareHandlers();
                if (reason != SpellFailedReason.Ok)
                    this.Cancel(reason);
            }

            return reason;
        }

        private SpellFailedReason Prepare()
        {
            if (this.Spell == null)
            {
                SpellCast._log.Warn("{0} tried to cast without selecting a Spell.", (object) this.CasterObject);
                return SpellFailedReason.Error;
            }

            try
            {
                if (this.IsAICast)
                {
                    SpellFailedReason reason = this.PrepareAI();
                    if (reason != SpellFailedReason.Ok)
                    {
                        this.Cancel(reason);
                        return reason;
                    }
                }

                if (this.SelectedTarget == null && this.CasterUnit != null)
                    this.SelectedTarget = this.InitialTargets == null
                        ? (WorldObject) this.CasterUnit.Target
                        : this.InitialTargets[0];
                if (!this.IsPassive && !this.Spell.IsPassive && this.CasterUnit != null)
                {
                    Spell spell = this.Spell;
                    if (!this.GodMode && !this.IsPassive && this.CasterUnit.IsPlayer)
                    {
                        SpellFailedReason reason = this.CheckPlayerCast(this.SelectedTarget);
                        if (reason != SpellFailedReason.Ok)
                        {
                            this.Cancel(reason);
                            return reason;
                        }
                    }

                    this.CasterUnit.Auras.RemoveByFlag(AuraInterruptFlags.OnCast);
                }

                this.StartTime = Environment.TickCount;
                this.m_castDelay = 0;
                if (this.CasterUnit != null && DateTime.Now.AddMilliseconds((double) (int) this.Spell.CastDelay) >
                    this.CasterUnit.CastingTill)
                    this.CasterUnit.CastingTill = DateTime.Now.AddMilliseconds((double) (int) this.Spell.CastDelay);
                if (!this.IsInstant && this.CasterUnit != null)
                {
                    this.m_castDelay = MathUtil.RoundInt(this.CasterUnit.CastSpeedFactor * (float) this.m_castDelay);
                    this.m_castDelay =
                        this.CasterUnit.Auras.GetModifiedInt(SpellModifierType.CastTime, this.Spell, this.m_castDelay);
                }

                if (this.Spell.TargetLocation != null)
                    this.TargetLoc = this.Spell.TargetLocation.Position;
                return this.Spell.NotifyCasting(this);
            }
            catch (Exception ex)
            {
                this.OnException(ex);
                return SpellFailedReason.Error;
            }
        }

        private SpellFailedReason FinishPrepare()
        {
            try
            {
                if (!this.IsInstant)
                {
                    if (this.CasterObject is Unit)
                        ((Unit) this.CasterObject).SheathType = SheathType.None;
                    this.m_castTimer.Start(this.m_castDelay);
                    return SpellFailedReason.Ok;
                }

                if (!this.Spell.IsOnNextStrike)
                    return this.Perform();
                if (!(this.CasterObject is Unit))
                {
                    this.Cancel(SpellFailedReason.Interrupted);
                    return SpellFailedReason.Error;
                }

                this.CasterUnit.SetSpellCast(this);
                return SpellFailedReason.Ok;
            }
            catch (Exception ex)
            {
                this.OnException(ex);
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
            if (!this.Spell.IsVisibleToClient)
                return;
            SpellHandler.SendCastStart(this);
        }

        internal void SendSpellGo(List<MissedTarget> missedTargets)
        {
            if (this.Spell.IsPassive || this.Spell.Attributes.HasAnyFlag(SpellAttributes.InvisibleAura) ||
                (this.Spell.HasEffectWith((Predicate<SpellEffect>) (effect =>
                     effect.EffectType == SpellEffectType.OpenLock)) || !this.Spell.IsVisibleToClient))
                return;
            byte previousRuneMask = this.UsesRunes ? this.CasterChar.PlayerSpells.Runes.GetActiveRuneMask() : (byte) 0;
            SpellHandler.SendSpellGo((IEntity) ((object) this.CasterItem ?? (object) this.CasterReference), this,
                (ICollection<WorldObject>) this.Targets, (ICollection<MissedTarget>) missedTargets, previousRuneMask);
        }

        /// <summary>Checks the current Cast when Players are using it</summary>
        protected SpellFailedReason CheckPlayerCast(WorldObject selected)
        {
            Character casterChar = this.CasterChar;
            if (!this.IsAoE && casterChar != selected && selected != null)
            {
                if (this.Spell.HasHarmfulEffects && selected is Unit &&
                    (((Unit) selected).IsEvading || ((Unit) selected).IsInvulnerable))
                    return SpellFailedReason.TargetAurastate;
            }
            else if (!casterChar.IsAlive)
                return SpellFailedReason.CasterDead;

            SpellFailedReason spellFailedReason = this.Spell.CheckCasterConstraints((Unit) casterChar);
            if (spellFailedReason != SpellFailedReason.Ok)
                return spellFailedReason;
            casterChar.CancelLooting();
            if (this.RequiredSkillIsTooLow(casterChar))
                return SpellFailedReason.MinSkill;
            if (this.Spell.IsTame)
            {
                NPC target = selected as NPC;
                if (target == null)
                    return SpellFailedReason.BadTargets;
                if (target.CurrentTamer != null)
                    return SpellFailedReason.AlreadyBeingTamed;
                if (SpellCast.CheckTame(casterChar, target) != TameFailReason.Ok)
                    return SpellFailedReason.DontReport;
            }

            if (!casterChar.HasEnoughPowerToCast(this.Spell, (WorldObject) null) ||
                this.UsesRunes && !casterChar.PlayerSpells.Runes.HasEnoughRunes(this.Spell))
                return SpellFailedReason.NoPower;
            return this.Spell.CheckItemRestrictions(this.TargetItem, casterChar.Inventory);
        }

        private bool RequiredSkillIsTooLow(Character caster)
        {
            if (this.Spell.Ability != null && this.Spell.Ability.RedValue > 0U)
                return caster.Skills.GetValue(this.Spell.Ability.Skill.Id) < this.Spell.Ability.RedValue;
            return false;
        }

        /// <summary>Check if SpellCast hit the targets.</summary>
        /// <remarks>Never returns null</remarks>
        private List<MissedTarget> CheckHit()
        {
            List<MissedTarget> missedTargetList = SpellCast.CastMissListPool.Obtain();
            if (this.GodMode || this.Spell.IsPassive || this.Spell.IsPhysicalAbility)
                return missedTargetList;
            this.hitChecker.Initialize(this.Spell, this.CasterObject);
            foreach (Unit target in this.UnitTargets.Where<Unit>((Func<Unit, bool>) (target =>
                !this.Spell.IsBeneficialFor(this.CasterReference, (WorldObject) target))))
            {
                CastMissReason reason = this.hitChecker.CheckHitAgainstTarget(target);
                if (reason != CastMissReason.None)
                    missedTargetList.Add(new MissedTarget((WorldObject) target, reason));
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
            if (!target.IsAlive)
                reason = TameFailReason.TargetDead;
            else if (!target.Entry.IsTamable)
                reason = TameFailReason.NotTamable;
            else if (target.Entry.IsExoticPet && !caster.CanControlExoticPets)
                reason = TameFailReason.CantControlExotic;
            else if (target.HasMaster)
            {
                reason = TameFailReason.CreatureAlreadyOwned;
            }
            else
            {
                if (target.Level <= caster.Level)
                    return TameFailReason.Ok;
                reason = TameFailReason.TooHighLevel;
            }

            if (caster != null)
                PetHandler.SendTameFailure((IPacketReceiver) caster, reason);
            return reason;
        }

        /// <summary>Tries to consume the given amount of power</summary>
        public SpellFailedReason ConsumePower(int amount)
        {
            Unit casterUnit = this.CasterUnit;
            if (casterUnit != null)
            {
                if (this.Spell.PowerType != PowerType.Health)
                {
                    if (!casterUnit.ConsumePower(this.Spell.Schools[0], this.Spell, amount))
                        return SpellFailedReason.NoPower;
                }
                else
                {
                    int health = casterUnit.Health;
                    casterUnit.Health = health - amount;
                    if (amount >= health)
                        return SpellFailedReason.CasterDead;
                }
            }

            return SpellFailedReason.Ok;
        }

        public IAsda2Weapon GetWeapon()
        {
            if (!(this.CasterObject is Unit))
            {
                SpellCast._log.Warn("{0} is not a Unit and casted Weapon Ability {1}", (object) this.CasterObject,
                    (object) this.Spell);
                return (IAsda2Weapon) null;
            }

            IAsda2Weapon weapon = ((Unit) this.CasterObject).GetWeapon(this.Spell.EquipmentSlot);
            if (weapon == null)
                SpellCast._log.Warn("{0} casted {1} without required Weapon: {2}", (object) this.CasterObject,
                    (object) this.Spell, (object) this.Spell.EquipmentSlot);
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
            LogUtil.ErrorException(e, "{0} failed to cast Spell {1} (Targets: {2})", (object) this.CasterObject,
                (object) this.Spell, (object) this.Targets.ToString<WorldObject>(", "));
            if (this.CasterObject != null && !this.CasterObject.IsPlayer)
                this.CasterObject.Delete();
            else if (this.Client != null)
                this.Client.Disconnect(false);
            if (!this.IsCasting)
                return;
            this.Cleanup();
        }

        public void Pushback(int millis)
        {
            if (this.IsChanneling)
                this.Channel.Pushback(millis);
            else
                this.RemainingCastTime += millis;
        }

        /// <summary>
        /// Caused by damage.
        /// Delays the cast and might result in interruption (only if not DoT).
        /// See: http://www.wowwiki.com/Interrupt
        /// </summary>
        public void Pushback()
        {
            if (this.GodMode || !this.IsCasting)
                return;
            if (this.Spell.InterruptFlags.HasFlag((Enum) InterruptFlags.OnTakeDamage))
            {
                this.Cancel(SpellFailedReason.Interrupted);
            }
            else
            {
                if (this.m_pushbacks >= 2 || this.RemainingCastTime <= 0)
                    return;
                if (this.IsChanneling)
                    this.Channel.Pushback(
                        this.GetPushBackTime(this.Channel.Duration / SpellCast.ChannelPushbackFraction));
                else
                    this.RemainingCastTime += this.GetPushBackTime(SpellCast.PushbackDelay);
                ++this.m_pushbacks;
            }
        }

        private int GetPushBackTime(int time)
        {
            if (this.CasterObject is Unit)
            {
                int spellInterruptProt = ((Unit) this.CasterObject).GetSpellInterruptProt(this.Spell);
                if (spellInterruptProt >= 100)
                    return 0;
                time -= spellInterruptProt * time / 100;
                time = ((Unit) this.CasterObject).Auras.GetModifiedIntNegative(SpellModifierType.PushbackReduction,
                    this.Spell, time);
            }

            return Math.Max(0, time);
        }

        public void Trigger(SpellId spell)
        {
            this.Trigger(SpellHandler.Get(spell), new WorldObject[0]);
        }

        public void TriggerSelf(SpellId spell)
        {
            this.TriggerSingle(SpellHandler.Get(spell), this.CasterObject);
        }

        public void TriggerSelf(Spell spell)
        {
            this.TriggerSingle(spell, this.CasterObject);
        }

        public void TriggerSingle(SpellId spell, WorldObject singleTarget)
        {
            this.TriggerSingle(SpellHandler.Get(spell), singleTarget);
        }

        /// <summary>
        /// Casts the given spell on the given target, inheriting this SpellCast's information.
        /// SpellCast will automatically be enqueued if the Character is currently not in the world.
        /// </summary>
        public void TriggerSingle(Spell spell, WorldObject singleTarget)
        {
            SpellCast cast = this.InheritSpellCast();
            int num;
            this.ExecuteInContext((Action) (() => num = (int) cast.Start(spell, true, singleTarget)));
        }

        /// <summary>
        /// Triggers all given spells instantly on the given single target
        /// </summary>
        public void TriggerAll(WorldObject singleTarget, params Spell[] spells)
        {
            if (this.CasterObject is Character && !this.CasterObject.IsInWorld)
                this.CasterChar.AddMessage(
                    (IMessage) new Message((Action) (() => this.TriggerAllSpells(singleTarget, spells))));
            else
                this.TriggerAllSpells(singleTarget, spells);
        }

        private void TriggerAllSpells(WorldObject singleTarget, params Spell[] spells)
        {
            SpellCast cast = SpellCast.SpellCastPool.Obtain();
            foreach (Spell spell in spells)
            {
                this.SetupInheritedCast(cast);
                int num = (int) cast.Start(spell, true, singleTarget);
            }
        }

        /// <summary>
        /// Casts the given spell on the given targets within this SpellCast's context.
        /// Finds targets automatically if the given targets are null.
        /// </summary>
        public void Trigger(SpellId spell, params WorldObject[] targets)
        {
            this.Trigger(spell, (SpellEffect) null, targets);
        }

        /// <summary>
        /// Casts the given spell on the given targets within this SpellCast's context.
        /// Finds targets automatically if the given targets are null.
        /// </summary>
        public void Trigger(SpellId spell, SpellEffect triggerEffect, params WorldObject[] targets)
        {
            this.Trigger(spell, triggerEffect, (IUnitAction) null, targets);
        }

        /// <summary>
        /// Casts the given spell on the given targets within this SpellCast's context.
        /// Finds targets automatically if the given targets are null.
        /// </summary>
        public void Trigger(SpellId spell, SpellEffect triggerEffect, IUnitAction triggerAction = null,
            params WorldObject[] targets)
        {
            this.Trigger(SpellHandler.Get(spell), triggerEffect, triggerAction, targets);
        }

        /// <summary>
        /// Casts the given spell on the given targets within this SpellCast's context.
        /// Determines targets and hostility, based on the given triggerEffect.
        /// </summary>
        public void Trigger(Spell spell, SpellEffect triggerEffect, params WorldObject[] initialTargets)
        {
            this.Trigger(spell, triggerEffect, (IUnitAction) null, initialTargets);
        }

        /// <summary>
        /// Casts the given spell on the given targets within this SpellCast's context.
        /// Determines targets and hostility, based on the given triggerEffect.
        /// </summary>
        public void Trigger(Spell spell, SpellEffect triggerEffect, IUnitAction triggerAction,
            params WorldObject[] initialTargets)
        {
            SpellCast cast = this.InheritSpellCast();
            cast.TriggerAction = triggerAction;
            int num;
            this.ExecuteInContext((Action) (() => num = (int) cast.Start(spell, triggerEffect, true, initialTargets)));
        }

        /// <summary>
        /// Casts the given spell on the given targets within this SpellCast's context.
        /// Finds targets automatically if the given targets are null.
        /// </summary>
        public void Trigger(Spell spell, params WorldObject[] targets)
        {
            SpellCast cast = this.InheritSpellCast();
            int num;
            this.ExecuteInContext((Action) (() => num = (int) cast.Start(spell, true,
                targets == null || targets.Length <= 0 ? (WorldObject[]) null : targets)));
        }

        /// <summary>
        /// Casts the given spell on targets determined by the given Spell.
        /// The given selected object will be the target, if the spell is a single target spell.
        /// </summary>
        public void TriggerSelected(Spell spell, WorldObject selected)
        {
            SpellCast cast = this.InheritSpellCast();
            cast.SelectedTarget = selected;
            int num;
            this.ExecuteInContext((Action) (() => num = (int) cast.Start(spell, true)));
        }

        private SpellCast InheritSpellCast()
        {
            SpellCast cast = SpellCast.SpellCastPool.Obtain();
            this.SetupInheritedCast(cast);
            return cast;
        }

        private void SetupInheritedCast(SpellCast cast)
        {
            cast.SetCaster(this.CasterReference, this.Map, this.Phase, this.SourceLoc);
            cast.TargetLoc = this.TargetLoc;
            cast.SelectedTarget = this.SelectedTarget;
            cast.CasterItem = this.CasterItem;
        }

        public static void ValidateAndTriggerNew(Spell spell, Unit caster, WorldObject target,
            SpellChannel usedChannel = null, Item usedItem = null, IUnitAction action = null,
            SpellEffect triggerEffect = null)
        {
            SpellCast.ValidateAndTriggerNew(spell, caster.SharedReference, caster, target, usedChannel, usedItem,
                action, triggerEffect);
        }

        public static void ValidateAndTriggerNew(Spell spell, ObjectReference caster, Unit triggerOwner,
            WorldObject target, SpellChannel usedChannel = null, Item usedItem = null, IUnitAction action = null,
            SpellEffect triggerEffect = null)
        {
            SpellCast spellCast = SpellCast.SpellCastPool.Obtain();
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
            this.InheritSpellCast().ValidateAndTrigger(spell, triggerOwner, target, action, triggerEffect);
        }

        public void ValidateAndTrigger(Spell spell, Unit triggerOwner, IUnitAction action = null)
        {
            if (action != null)
            {
                ++action.ReferenceCount;
                this.TriggerAction = action;
            }

            this.ValidateAndTrigger(spell, triggerOwner, (WorldObject) null, action, (SpellEffect) null);
        }

        public void ValidateAndTrigger(Spell spell, Unit triggerOwner, WorldObject target, IUnitAction action = null,
            SpellEffect triggerEffect = null)
        {
            if (triggerOwner == null)
            {
                SpellCast._log.Warn("triggerOwner is null when trying to proc spell: {0} (target: {1})", (object) spell,
                    (object) target);
            }
            else
            {
                WorldObject[] worldObjectArray;
                if (spell.CasterIsTarget || !spell.HasTargets)
                    worldObjectArray = (WorldObject[]) new Unit[1]
                    {
                        triggerOwner
                    };
                else if (target != null)
                {
                    if (spell.IsAreaSpell || this.CasterObject == null ||
                        spell.IsHarmfulFor(this.CasterReference, target) !=
                        target.IsHostileWith((IFactionMember) this.CasterObject))
                        worldObjectArray = (WorldObject[]) null;
                    else
                        worldObjectArray = new WorldObject[1] {target};
                }
                else
                    worldObjectArray = (WorldObject[]) null;

                if (action != null)
                {
                    ++action.ReferenceCount;
                    this.TriggerAction = action;
                }

                int num = (int) this.Start(spell, triggerEffect, true, worldObjectArray);
            }
        }

        public void Cancel(SpellFailedReason reason = SpellFailedReason.Interrupted)
        {
            if (!this.IsCasting)
                return;
            this.IsCasting = false;
            this.CloseChannel();
            this.Spell.NotifyCancelled(this, reason);
            if (reason != SpellFailedReason.Ok)
            {
                if (!this.IsPassive && this.Spell.IsVisibleToClient)
                {
                    if (reason != SpellFailedReason.OutOfRange && this.CasterChar != null)
                        this.CasterChar.SendSystemMessage(string.Format("Cast spell failed cause : {0}",
                            (object) reason));
                }
                else if (this.CasterObject != null && this.CasterObject.IsUsingSpell &&
                         reason != SpellFailedReason.DontReport)
                    this.CancelOriginalSpellCast(reason);
            }

            this.Cleanup();
        }

        private void CloseChannel()
        {
            if (this.Channel == null)
                return;
            this.Channel.Close(true);
        }

        private void CancelOriginalSpellCast(SpellFailedReason reason)
        {
            SpellCast spellCast = this.CasterObject.SpellCast;
            if (this == this.CasterObject.SpellCast)
                return;
            spellCast.Cancel(reason);
        }

        public void Update(int dt)
        {
            this.m_castTimer.Update(dt);
            if (!this.IsChanneling)
                return;
            this.Channel.Update(dt);
        }

        /// <summary>
        /// Close the timer and get rid of circular references; will be called automatically
        /// </summary>
        protected internal void Cleanup()
        {
            this.IsPlayerCast = false;
            this.IsCasting = false;
            if (this.Spell.IsTame && this.SelectedTarget is NPC)
                ((NPC) this.SelectedTarget).CurrentTamer = (Character) null;
            this.TargetItem = (Item) null;
            this.CasterItem = (Item) null;
            this.m_castTimer.Stop();
            this.InitialTargets = (WorldObject[]) null;
            this.Handlers = (SpellEffectHandler[]) null;
            this.IsPassive = false;
            this.m_pushbacks = 0;
            this.Spell = (Spell) null;
            this.TriggerEffect = (SpellEffect) null;
            this.TargetFlags = SpellTargetFlags.Self;
            this.Targets.Clear();
            this.FinalCleanup();
        }

        private void FinalCleanup()
        {
            this.CleanupTriggerAction();
            this.CleanupHandlers();
            if (this.CasterObject != null && this.CasterObject.SpellCast == this)
                return;
            this.Dispose();
        }

        private void CleanupTriggerAction()
        {
            if (this.TriggerAction == null)
                return;
            --this.TriggerAction.ReferenceCount;
            this.TriggerAction = (IUnitAction) null;
        }

        private void CleanupHandlers()
        {
            if (this.Handlers == null)
                return;
            foreach (SpellEffectHandler spellEffectHandler in ((IEnumerable<SpellEffectHandler>) this.Handlers)
                .Where<SpellEffectHandler>((Func<SpellEffectHandler, bool>) (handler => handler != null)))
                spellEffectHandler.Cleanup();
        }

        internal void Dispose()
        {
            if (this.CasterReference == null)
            {
                SpellCast._log.Warn("Tried to dispose SpellCast twice: " + (object) this);
            }
            else
            {
                this.Cancel(SpellFailedReason.Interrupted);
                if (this.Channel != null)
                {
                    this.Channel.Dispose();
                    this.Channel = (SpellChannel) null;
                }

                this.Targets.Clear();
                this.SourceLoc = Vector3.Zero;
                this.CasterObject = (WorldObject) (this.CasterUnit = (Unit) null);
                this.CasterReference = (ObjectReference) null;
                this.Map = (Map) null;
                this.SelectedTarget = (WorldObject) null;
                this.GodMode = false;
                SpellCast.SpellCastPool.Recycle(this);
            }
        }

        public void SendPacketToArea(RealmPacketOut packet)
        {
            if (this.CasterObject != null)
                this.CasterObject.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            else
                this.Map.SendPacketToArea(packet, ref this.m_targetLoc, this.Phase);
        }

        /// <summary>
        /// Is called whenever the validy of the caster might have changed
        /// </summary>
        private void CheckCasterValidity()
        {
            if (this.CasterObject == null || this.CasterObject.IsInWorld && this.CasterObject.IsInContext)
                return;
            this.CasterObject = (WorldObject) null;
            this.CasterUnit = (Unit) null;
        }

        /// <summary>Remove target from targets set and handler targets</summary>
        /// <param name="target"></param>
        private void Remove(WorldObject target)
        {
            this.Targets.Remove(target);
            this.RemoveFromHandlerTargets(target);
        }

        private void RemoveFromHandlerTargets(WorldObject target)
        {
            foreach (SpellEffectHandler handler in this.Handlers)
                handler.m_targets.Remove(target);
        }

        private void RemoveFromHandlerTargets(List<MissedTarget> missedTargets)
        {
            foreach (MissedTarget missedTarget in missedTargets)
                this.RemoveFromHandlerTargets(missedTarget.Target);
        }

        public SpellEffectHandler GetHandler(SpellEffectType type)
        {
            if (this.Handlers == null)
                throw new InvalidOperationException("Tried to get Handler from unintialized SpellCast");
            return ((IEnumerable<SpellEffectHandler>) this.Handlers).FirstOrDefault<SpellEffectHandler>(
                (Func<SpellEffectHandler, bool>) (handler => handler.Effect.EffectType == type));
        }

        public override string ToString()
        {
            return this.Spell.ToString() + " casted by " + (object) this.CasterObject;
        }
    }
}