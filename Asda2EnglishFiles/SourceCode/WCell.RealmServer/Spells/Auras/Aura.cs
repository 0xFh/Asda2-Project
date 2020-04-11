using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Timers;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>
    /// An Aura is any kind of long-lasting passive effect or buff.
    /// Some can be seen as an icon below the Player's status bar.
    /// </summary>
    public class Aura : IAura, IUpdatable, IProcHandler, IDisposable, ITickTimer
    {
        public static readonly Aura[] EmptyArray = new Aura[0];
        public static readonly IEnumerator<Aura> EmptyEnumerator = (IEnumerator<Aura>) new Aura.AuraEnumerator();
        public readonly AuraIndexId Id;
        protected internal AuraCollection m_auras;
        protected ObjectReference m_CasterReference;
        protected Spell m_spell;
        protected List<AuraEffectHandler> m_handlers;
        protected bool m_beneficial;

        /// <summary>
        /// The controlling Timer (eg a SpellChannel) or null if self-controlled
        /// </summary>
        protected ITickTimer m_controller;

        protected int m_stackCount;
        protected int m_startTime;
        protected int m_duration;
        protected int m_amplitude;
        protected int m_ticks;
        protected int m_maxTicks;
        private TimerEntry m_timer;
        protected byte m_index;
        protected AuraFlags m_auraFlags;
        protected byte m_auraLevel;
        protected AuraRecord m_record;
        private Item m_UsedItem;
        private bool m_hasPeriodicallyUpdatedEffectHandler;
        private bool m_IsActivated;

        private Aura()
        {
        }

        /// <summary>Creates a new Aura</summary>
        /// <param name="auras"></param>
        /// <param name="casterReference">Information about who casted</param>
        /// <param name="spell">The spell that this Aura represents</param>
        /// <param name="handlers">All handlers must have the same AuraUID</param>
        internal Aura(AuraCollection auras, ObjectReference casterReference, Spell spell,
            List<AuraEffectHandler> handlers, byte index, bool beneficial)
        {
            this.m_auras = auras;
            this.m_spell = spell;
            this.m_beneficial = beneficial;
            this.Id = spell.GetAuraUID(beneficial);
            this.m_handlers = handlers;
            this.m_CasterReference = casterReference;
            this.m_index = index;
            this.m_auraLevel = (byte) casterReference.Level;
            this.m_stackCount = (int) (byte) this.m_spell.InitialStackCount;
            if (this.m_stackCount > 0 && casterReference.UnitMaster != null)
                this.m_stackCount =
                    casterReference.UnitMaster.Auras.GetModifiedInt(SpellModifierType.Charges, this.m_spell,
                        this.m_stackCount);
            this.SetupValues();
        }

        internal Aura(AuraCollection auras, ObjectReference caster, AuraRecord record, List<AuraEffectHandler> handlers,
            byte index)
        {
            this.m_record = record;
            this.m_auras = auras;
            this.m_spell = record.Spell;
            this.m_beneficial = record.IsBeneficial;
            this.Id = this.m_spell.GetAuraUID(this.m_beneficial);
            this.m_handlers = handlers;
            this.m_CasterReference = caster;
            this.m_index = index;
            this.m_auraLevel = (byte) record.Level;
            this.m_stackCount = record.StackCount;
            this.SetupValues();
            this.m_duration = record.MillisLeft;
            this.SetupTimer();
        }

        /// <summary>
        /// Called after setting up the Aura and before calling Start()
        /// </summary>
        private void SetupTimer()
        {
            if (this.m_controller != null || this.m_amplitude <= 0 && this.m_duration <= 0)
                return;
            this.m_timer = new TimerEntry()
            {
                Action = new Action<int>(this.Apply)
            };
        }

        private void SetupValues()
        {
            this.DetermineFlags();
            this.m_hasPeriodicallyUpdatedEffectHandler = this.m_handlers.Any<AuraEffectHandler>(
                (Func<AuraEffectHandler, bool>) (handler => handler is PeriodicallyUpdatedAuraEffectHandler));
            if (this.m_amplitude != 0)
                return;
            foreach (AuraEffectHandler handler in this.m_handlers)
            {
                if (handler.SpellEffect.Amplitude > 0)
                {
                    this.m_amplitude = handler.SpellEffect.Amplitude;
                    break;
                }
            }
        }

        private void DetermineFlags()
        {
            this.m_auraFlags = this.m_spell.DefaultAuraFlags;
            if (this.m_auras.Owner.EntityId == this.m_CasterReference.EntityId)
                this.m_auraFlags |= AuraFlags.TargetIsCaster;
            if (this.m_beneficial)
                this.m_auraFlags |= AuraFlags.Positive;
            else
                this.m_auraFlags |= AuraFlags.Negative;
            if (this.m_spell.Durations.Min > 0)
                this.m_auraFlags |= AuraFlags.HasDuration;
            for (int index = Math.Min(this.m_handlers.Count - 1, 2); index >= 0; --index)
            {
                int effectIndex = (int) this.m_handlers[index].SpellEffect.EffectIndex;
                if (effectIndex >= 0)
                    this.m_auraFlags |= (AuraFlags) (1 << effectIndex);
            }

            if (this.m_auraFlags != AuraFlags.None)
                return;
            this.m_auraFlags = AuraFlags.Effect1AppliesAura;
        }

        /// <summary>
        /// The <c>AuraCollection</c> of the Unit that owns this Aura.
        /// </summary>
        public AuraCollection Auras
        {
            get { return this.m_auras; }
        }

        /// <summary>The Spell that belongs to this Aura</summary>
        public Spell Spell
        {
            get { return this.m_spell; }
        }

        /// <summary>The amount of times that this Aura has been applied</summary>
        public int StackCount
        {
            get { return this.m_stackCount; }
            set { this.m_stackCount = value; }
        }

        /// <summary>Whether this Aura is added to it's owner</summary>
        public bool IsAdded { get; protected internal set; }

        public bool CanBeRemoved
        {
            get
            {
                if (this.m_spell != null && this.m_beneficial &&
                    !this.m_spell.AttributesEx.HasAnyFlag(SpellAttributesEx.Negative))
                    return !this.m_spell.Attributes.HasAnyFlag(SpellAttributes.CannotRemove);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsBeneficial
        {
            get { return this.m_beneficial; }
        }

        /// <summary>
        /// The amount of ticks left (always 0 for non-periodic auras)
        /// </summary>
        public int TicksLeft
        {
            get { return this.MaxTicks - this.Ticks; }
        }

        /// <summary>Information about the caster</summary>
        public ObjectReference CasterReference
        {
            get { return this.m_CasterReference; }
        }

        /// <summary>
        /// The actual Caster (returns null if caster went offline or disappeared for some other reason)
        /// </summary>
        public Unit CasterUnit
        {
            get
            {
                Unit unitMaster = this.m_CasterReference.UnitMaster;
                if (unitMaster != null && unitMaster.IsInContext)
                    return unitMaster;
                return (Unit) null;
            }
        }

        /// <summary>
        /// The SpellCast that caused this Aura (if still present)
        /// </summary>
        public SpellCast SpellCast
        {
            get
            {
                SpellChannel controller = this.Controller as SpellChannel;
                if (controller != null)
                    return controller.Cast;
                return this.CasterUnit?.SpellCast;
            }
        }

        public Unit Owner
        {
            get { return this.m_auras.Owner; }
        }

        public Item UsedItem
        {
            get
            {
                if (this.m_UsedItem != null && this.m_UsedItem.IsInWorld && this.m_UsedItem.IsInContext)
                    return this.m_UsedItem;
                return (Item) null;
            }
            internal set { this.m_UsedItem = value; }
        }

        /// <summary>
        ///  The amplitude between aura-ticks (only for non-passive auras which are not channeled)
        /// </summary>
        public int Amplitude
        {
            get { return this.m_amplitude; }
        }

        /// <summary>
        /// Whether this Aura is not visible to the client (only its effects will make him realize it)
        /// </summary>
        public bool IsVisible
        {
            get
            {
                if (this.m_spell.IsPassive && !this.m_spell.AttributesEx.HasFlag((Enum) SpellAttributesEx.Negative))
                    return this.m_CasterReference.Object != this.m_auras.Owner;
                return true;
            }
        }

        /// <summary>The maximum amount of Applications for this Aura</summary>
        public int MaxApplications
        {
            get { return this.m_spell.MaxStackCount; }
        }

        /// <summary>
        /// The position of this Aura within the client's Aura-bar (0 if not exposed to client)
        /// </summary>
        public byte Index
        {
            get { return this.m_index; }
            set
            {
                this.RemoveFromClient();
                this.m_index = value;
                this.SendToClient();
            }
        }

        /// <summary>
        /// Time that is left until this Aura disbands in millis.
        /// Auras's without timeout can't be resetted.
        /// Channeled Auras are controlled by the holding SpellChannel.
        /// Returns a negative value if Aura doesn't has a timeout (or is already expired).
        /// </summary>
        public int TimeLeft
        {
            get
            {
                if (this.m_controller == null)
                    return this.m_duration - (Environment.TickCount - this.m_startTime);
                return this.m_controller.TimeLeft;
            }
            set
            {
                if (!this.HasTimer)
                    return;
                this.m_startTime = Environment.TickCount;
                int initialDelay;
                if (this.m_amplitude > 0)
                {
                    if (value <= 0)
                    {
                        this.m_maxTicks = int.MaxValue;
                    }
                    else
                    {
                        this.m_maxTicks = value / this.m_amplitude;
                        if (this.m_maxTicks < 1)
                            this.m_maxTicks = 1;
                    }

                    initialDelay = value % (this.m_amplitude + 1);
                }
                else
                {
                    initialDelay = value;
                    this.m_maxTicks = 1;
                }

                this.m_ticks = 0;
                if (value < 0)
                    this.m_timer.Stop();
                else
                    this.m_timer.Start(initialDelay);
            }
        }

        /// <summary>Wheter this Aura can be saved</summary>
        public bool CanBeSaved { get; set; }

        /// <summary>
        /// Whether it is safe and legal to steal this Aura (only temporary Auras that are not controlled by a channel or similar)
        /// </summary>
        public bool CanBeStolen
        {
            get
            {
                if (this.HasTimeout)
                    return !this.m_spell.IsTriggeredSpell;
                return false;
            }
        }

        public IEnumerable<AuraEffectHandler> Handlers
        {
            get { return (IEnumerable<AuraEffectHandler>) this.m_handlers; }
        }

        /// <summary>
        /// The controller of this Aura which controls timing, application and removal (such as <see cref="T:WCell.RealmServer.Spells.SpellChannel">SpellChannels</see>)
        /// </summary>
        public ITickTimer Controller
        {
            get { return this.m_controller; }
        }

        /// <summary>
        /// Auras that are not passive and not controlled by a <c>ITickTimer</c> have their own Timers
        /// </summary>
        public bool HasTimeout
        {
            get
            {
                if (this.m_spell.Durations.Min > 0)
                    return this.m_controller == null;
                return false;
            }
        }

        public bool HasTimer
        {
            get { return this.m_timer != null; }
        }

        public int Ticks
        {
            get
            {
                if (this.m_controller != null)
                    return this.m_controller.Ticks;
                return this.m_ticks;
            }
        }

        public int MaxTicks
        {
            get
            {
                if (this.m_controller != null)
                    return this.m_controller.MaxTicks;
                return this.m_maxTicks;
            }
        }

        /// <summary>Duration in millis</summary>
        public int Duration
        {
            get
            {
                if (this.m_controller != null)
                    return this.m_controller.Duration;
                return this.m_duration;
            }
            set
            {
                this.m_duration = value;
                this.m_auraFlags |= AuraFlags.HasDuration;
                this.SetupTimer();
                this.TimeLeft = this.m_duration;
            }
        }

        public int Until
        {
            get
            {
                if (this.m_spell.IsPassive)
                    return -1;
                if (this.m_controller != null)
                    return this.m_controller.Until;
                return Environment.TickCount - this.m_startTime;
            }
        }

        public byte Level
        {
            get { return this.m_auraLevel; }
        }

        public AuraFlags Flags
        {
            get { return this.m_auraFlags; }
        }

        public bool HasPeriodicallyUpdatedEffectHandler
        {
            get { return this.m_hasPeriodicallyUpdatedEffectHandler; }
        }

        /// <summary>Method is called</summary>
        /// <param name="noTimeout">Whether the Aura should always continue and never expire.</param>
        public void Start(ITickTimer controller, bool noTimeout)
        {
            this.m_controller = controller;
            this.m_duration = !noTimeout ? this.Spell.GetDuration(this.m_CasterReference, this.m_auras.Owner) : -1;
            this.SetupTimer();
            this.Start();
        }

        public void Start()
        {
            this.TimeLeft = this.m_duration;
            foreach (AuraEffectHandler handler in this.m_handlers)
                handler.Init(this);
            if (this.m_auras.MayActivate(this))
                this.IsActivated = true;
            this.CanBeSaved = this != this.m_auras.GhostAura &&
                              !this.m_spell.AttributesExC.HasFlag((Enum) SpellAttributesExC.HonorlessTarget) &&
                              this.UsedItem == null;
            this.m_auras.OnAuraChange(this);
            Unit casterUnit = this.CasterUnit;
            Unit owner = this.Owner;
        }

        /// <summary>Disables the Aura without removing it's effects</summary>
        public bool IsActivated
        {
            get { return this.m_IsActivated; }
            set
            {
                if (this.m_IsActivated == value)
                    return;
                if (this.m_IsActivated = value)
                    this.Activate();
                else
                    this.Deactivate(false);
            }
        }

        private void Activate()
        {
            if (this.m_spell.IsProc && this.CasterUnit != null && this.m_spell.ProcHandlers != null)
            {
                foreach (ProcHandlerTemplate procHandler in this.m_spell.ProcHandlers)
                    this.Owner.AddProcHandler((IProcHandler) new ProcHandler(this.CasterUnit, this.Owner, procHandler));
            }

            if (this.m_spell.IsAuraProcHandler)
                this.m_auras.Owner.AddProcHandler((IProcHandler) this);
            if (this.m_spell.IsAreaAura && this.Owner.EntityId == this.CasterReference.EntityId)
            {
                AreaAura areaAura = this.m_auras.Owner.GetAreaAura(this.m_spell);
                if (areaAura != null)
                    areaAura.Start(this.m_controller, !this.HasTimeout);
            }

            this.ApplyNonPeriodicEffects();
            this.SendToClient();
        }

        /// <summary>Called when the Aura gets deactivated</summary>
        /// <param name="cancelled"></param>
        private void Deactivate(bool cancelled)
        {
            if (this.m_spell.ProcHandlers != null && this.CasterUnit != null)
            {
                foreach (ProcHandlerTemplate procHandler in this.m_spell.ProcHandlers)
                    this.Owner.RemoveProcHandler(procHandler);
            }

            if (this.m_spell.IsAuraProcHandler)
                this.m_auras.Owner.RemoveProcHandler((IProcHandler) this);
            if (this.m_spell.IsAreaAura && this.Owner.EntityId == this.CasterReference.EntityId)
            {
                AreaAura areaAura = this.m_auras.Owner.GetAreaAura(this.m_spell);
                if (areaAura != null)
                    areaAura.IsActivated = false;
            }

            this.CallAllHandlers((Aura.HandlerDelegate) (handler => handler.DoRemove(cancelled)));
            this.RemoveFromClient();
        }

        /// <summary>Applies this Aura's effect to its holder</summary>
        public void Apply()
        {
            this.Apply(0);
        }

        /// <summary>Applies one of this Aura's Ticks to its holder</summary>
        internal void Apply(int timeElapsed)
        {
            ++this.m_ticks;
            bool flag = (!this.m_spell.HasPeriodicAuraEffects || this.m_ticks >= this.m_maxTicks) &&
                        this.m_controller == null;
            if (this.m_IsActivated)
            {
                this.OnApply();
                this.ApplyPeriodicEffects();
                if (!this.IsAdded)
                    return;
                if (!flag && this.m_timer != null)
                    this.m_timer.Start(this.m_amplitude);
            }

            if (!flag)
                return;
            this.Remove(false);
        }

        /// <summary>
        /// Removes and then re-applies all non-perodic Aura-effects
        /// </summary>
        public void ReApplyNonPeriodicEffects()
        {
            this.RemoveNonPeriodicEffects();
            foreach (AuraEffectHandler handler in this.m_handlers)
                handler.UpdateEffectValue();
            this.ApplyNonPeriodicEffects();
        }

        /// <summary>Applies all non-perodic Aura-effects</summary>
        internal void ApplyNonPeriodicEffects()
        {
            if (!this.m_spell.HasNonPeriodicAuraEffects)
                return;
            foreach (AuraEffectHandler handler in this.Handlers)
            {
                if (!handler.SpellEffect.IsPeriodic && this.m_auras.MayActivate(handler))
                {
                    handler.DoApply();
                    if (!this.IsAdded)
                        break;
                }
            }
        }

        internal void ApplyPeriodicEffects()
        {
            if (!this.m_spell.HasPeriodicAuraEffects)
                return;
            foreach (AuraEffectHandler handler in this.m_handlers)
            {
                if (handler.SpellEffect.IsPeriodic && this.m_auras.MayActivate(handler))
                {
                    handler.DoApply();
                    if (!this.IsAdded)
                        break;
                }
            }
        }

        /// <summary>
        /// Do certain special behavior everytime an Aura is applied
        /// for very basic Aura categories.
        /// </summary>
        private void OnApply()
        {
            if (!this.m_spell.IsFood && !this.m_spell.IsDrink)
                return;
            this.CasterReference.UnitMaster.Emote(EmoteType.SimpleEat);
        }

        /// <summary>
        /// Refreshes this aura.
        /// If this Aura is stackable, will also increase the StackCount by one.
        /// </summary>
        public void Refresh(ObjectReference caster)
        {
            if (!this.IsAdded)
                return;
            this.RemoveNonPeriodicEffects();
            this.m_CasterReference = caster;
            if (this.m_spell.InitialStackCount > 1)
                this.m_stackCount = (int) (byte) this.m_spell.InitialStackCount;
            else if (this.m_stackCount < this.m_spell.MaxStackCount)
                ++this.m_stackCount;
            foreach (AuraEffectHandler handler in this.m_handlers)
                handler.UpdateEffectValue();
            this.ApplyNonPeriodicEffects();
            this.TimeLeft = this.m_spell.GetDuration(caster, this.m_auras.Owner);
            if (!this.IsVisible)
                return;
            AuraHandler.SendAuraUpdate(this.m_auras.Owner, this);
        }

        /// <summary>
        /// Checks all handlers and toggles those whose requirements aren't met
        /// </summary>
        internal void ReEvaluateNonPeriodicHandlerRequirements()
        {
            if (!this.Spell.HasNonPeriodicAuraEffects)
                return;
            foreach (AuraEffectHandler handler in this.Handlers)
            {
                if (!handler.SpellEffect.IsPeriodic)
                    handler.IsActivated = this.m_auras.MayActivate(handler);
            }
        }

        /// <summary>
        /// Stack or removes the given Aura, if possible.
        /// Returns whether the given incompatible Aura was removed or stacked.
        /// <param name="err">Ok, if stacked or no incompatible Aura was found</param>
        /// </summary>
        public Aura.AuraOverrideStatus GetOverrideStatus(ObjectReference caster, Spell spell)
        {
            if (this.Spell.IsPreventionDebuff)
                return Aura.AuraOverrideStatus.Bounced;
            if (this.Spell == spell)
                return Aura.AuraOverrideStatus.Refresh;
            if (caster == this.CasterReference)
                return spell != this.Spell ? Aura.AuraOverrideStatus.Replace : Aura.AuraOverrideStatus.Refresh;
            return !spell.CanOverride(this.Spell) ? Aura.AuraOverrideStatus.Bounced : Aura.AuraOverrideStatus.Refresh;
        }

        /// <summary>
        /// Removes and then re-applies all non-perodic Aura-effects
        /// </summary>
        private void RemoveNonPeriodicEffects()
        {
            if (!this.m_spell.HasNonPeriodicAuraEffects)
                return;
            foreach (AuraEffectHandler handler in this.m_handlers)
            {
                if (!handler.SpellEffect.IsPeriodic)
                    handler.IsActivated = false;
            }
        }

        public bool TryRemove(bool cancelled)
        {
            if (this.m_spell.IsAreaAura)
            {
                Unit owner = this.m_auras.Owner;
                if ((long) owner.EntityId.Low != (long) (ulong) this.CasterReference.EntityId &&
                    this.CasterUnit != null && this.CasterUnit.UnitMaster != owner)
                    return false;
                owner.CancelAreaAura(this.m_spell);
                return true;
            }

            this.Remove(cancelled);
            return true;
        }

        public void Cancel()
        {
            this.Remove(true);
        }

        internal void RemoveWithoutCleanup()
        {
            if (!this.IsAdded)
                return;
            this.IsAdded = false;
            this.Deactivate(true);
            if (this.m_controller != null)
                this.m_controller.OnRemove(this.Owner, this);
            this.OnRemove();
        }

        /// <summary>Removes this Aura from the player</summary>
        public void Remove(bool cancelled = true)
        {
            if (!this.IsAdded)
                return;
            this.IsAdded = false;
            Unit owner = this.m_auras.Owner;
            if (owner == null)
            {
                LogManager.GetCurrentClassLogger()
                    .Warn("Tried to remove Aura {0} but it's owner does not exist anymore.");
            }
            else
            {
                if (this.m_controller != null)
                    this.m_controller.OnRemove(owner, this);
                AuraCollection auras = this.m_auras;
                if (this.CasterUnit != null)
                    this.m_spell.NotifyAuraRemoved(this);
                auras.Remove(this);
                this.Deactivate(cancelled);
                this.OnRemove();
                if (!this.m_spell.IsAreaAura || !(owner.EntityId == this.CasterReference.EntityId))
                    return;
                owner.CancelAreaAura(this.m_spell);
            }
        }

        private void OnRemove()
        {
            if (this.m_record == null)
                return;
            this.m_record.DeleteLater();
            this.m_record = (AuraRecord) null;
        }

        /// <summary>
        /// Takes care of all the eye candy that is related to the removal of this Aura.
        /// </summary>
        protected void RemoveVisibleEffects(bool cancelled)
        {
        }

        /// <summary>
        /// Need to guarantee that all Auras that have ever been created will also be removed
        /// </summary>
        internal void Cleanup()
        {
            this.IsActivated = false;
            if (this.m_record == null)
                return;
            AuraRecord record = this.m_record;
            this.m_record = (AuraRecord) null;
            record.Recycle();
        }

        /// <summary>See IIAura.OnRemove</summary>
        public void OnRemove(Unit owner, Aura aura)
        {
            throw new NotImplementedException();
        }

        protected internal void SendToClient()
        {
            if (!this.IsVisible)
                return;
            AuraHandler.SendAuraUpdate(this.m_auras.Owner, this);
        }

        /// <summary>Removes all of this Aura's occupied fields</summary>
        protected void RemoveFromClient()
        {
            if (!this.IsVisible)
                return;
            Character owner1 = this.Owner as Character;
            NPC owner2 = this.Owner as NPC;
            if (owner2 != null)
                Asda2CombatHandler.SendMonstrStateChangedResponse(owner2, Asda2NpcState.Ok);
            if (owner1 == null)
                return;
            Asda2SpellHandler.SendBuffEndedResponse(owner1, this.Spell.RealId);
            if (owner1.IsInGroup)
                Asda2GroupHandler.SendPartyMemberBuffInfoResponse(owner1);
            if (owner1.SoulmateCharacter == null)
                return;
            Asda2SoulmateHandler.SendSoulmateBuffUpdateInfoResponse(owner1);
        }

        public void Update(int dt)
        {
            if (this.m_hasPeriodicallyUpdatedEffectHandler)
            {
                foreach (AuraEffectHandler handler in this.m_handlers)
                {
                    if (handler is PeriodicallyUpdatedAuraEffectHandler)
                        ((PeriodicallyUpdatedAuraEffectHandler) handler).Update();
                }
            }

            if (this.m_timer == null)
                return;
            this.m_timer.Update(dt);
        }

        public ProcTriggerFlags ProcTriggerFlags
        {
            get { return this.m_spell.ProcTriggerFlagsProp; }
        }

        public ProcHitFlags ProcHitFlags
        {
            get { return this.m_spell.ProcHitFlags; }
        }

        /// <summary>Spell to be triggered (if any)</summary>
        public Spell ProcSpell
        {
            get
            {
                if (this.m_spell.ProcTriggerEffects == null)
                    return (Spell) null;
                return this.m_spell.ProcTriggerEffects[0].TriggerSpell;
            }
        }

        /// <summary>Chance to proc in %</summary>
        public uint ProcChance
        {
            get
            {
                if (this.m_spell.ProcChance <= 0U)
                    return 100;
                return this.m_spell.ProcChance;
            }
        }

        public int MinProcDelay
        {
            get { return this.m_spell.ProcDelay; }
        }

        public DateTime NextProcTime { get; set; }

        public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
        {
            bool flag1 = this.m_spell.ProcTriggerEffects != null;
            bool flag2 = false;
            if (flag1)
            {
                foreach (AuraEffectHandler handler in this.m_handlers)
                {
                    if (handler.SpellEffect.IsProc && handler.CanProcBeTriggeredBy(action) &&
                        handler.SpellEffect.CanProcBeTriggeredBy(action.Spell))
                    {
                        flag2 = true;
                        break;
                    }
                }
            }
            else if (action.Spell == null || action.Spell != this.Spell)
                flag2 = true;

            if (flag2)
                return this.m_spell.CanProcBeTriggeredBy(this.m_auras.Owner, action, active);
            return false;
        }

        public void TriggerProc(Unit triggerer, IUnitAction action)
        {
            bool flag = false;
            if (this.m_spell.ProcTriggerEffects != null)
            {
                foreach (AuraEffectHandler handler in this.m_handlers)
                {
                    if (handler.SpellEffect.IsProc && handler.CanProcBeTriggeredBy(action) &&
                        handler.SpellEffect.CanProcBeTriggeredBy(action.Spell))
                    {
                        handler.OnProc(triggerer, action);
                        flag = true;
                    }
                }
            }
            else
                flag = true;

            if (!flag || this.m_spell.ProcCharges <= 0)
                return;
            --this.m_stackCount;
            if (this.m_stackCount == 0)
                this.Remove(false);
            else
                AuraHandler.SendAuraUpdate(this.m_auras.Owner, this);
        }

        public void Dispose()
        {
            this.Remove(false);
        }

        public void Save()
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(new Action(this.SaveNow));
        }

        internal void SaveNow()
        {
            if (this.m_record == null)
            {
                Unit owner = this.m_auras.Owner;
                if (!(owner is Character))
                    throw new InvalidOperationException(string.Format("Tried to save non-Player Aura {0} on: {1}",
                        (object) this, (object) owner));
                this.m_record = AuraRecord.ObtainAuraRecord(this);
            }
            else
                this.m_record.SyncData(this);

            this.m_record.Save();
        }

        protected void CallAllHandlers(Aura.HandlerDelegate dlgt)
        {
            foreach (AuraEffectHandler handler in this.m_handlers)
                dlgt(handler);
        }

        public AuraEffectHandler GetHandler(AuraType type)
        {
            foreach (AuraEffectHandler handler in this.Handlers)
            {
                if (handler.SpellEffect.AuraType == type)
                    return handler;
            }

            return (AuraEffectHandler) null;
        }

        public override string ToString()
        {
            return "Aura " + (object) this.m_spell + ": " +
                   (this.IsBeneficial ? (object) "Beneficial" : (object) "Harmful") +
                   (this.HasTimeout
                       ? (object) (" [TimeLeft: " + (object) TimeSpan.FromMilliseconds((double) this.TimeLeft) + "]")
                       : (object) "") + (this.m_controller != null
                       ? (object) (" Controlled by: " + (object) this.m_controller)
                       : (object) "");
        }

        public enum AuraOverrideStatus
        {
            NotPresent,
            Replace,
            Refresh,
            Bounced,
        }

        protected delegate void HandlerDelegate(AuraEffectHandler handler);

        private class AuraEnumerator : IEnumerator<Aura>, IDisposable, IEnumerator
        {
            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }

            public Aura Current
            {
                get { return (Aura) null; }
            }

            object IEnumerator.Current
            {
                get { return (object) null; }
            }
        }
    }
}