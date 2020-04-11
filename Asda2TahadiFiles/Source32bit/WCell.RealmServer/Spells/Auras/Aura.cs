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
    public static readonly IEnumerator<Aura> EmptyEnumerator = new AuraEnumerator();
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
      m_auras = auras;
      m_spell = spell;
      m_beneficial = beneficial;
      Id = spell.GetAuraUID(beneficial);
      m_handlers = handlers;
      m_CasterReference = casterReference;
      m_index = index;
      m_auraLevel = (byte) casterReference.Level;
      m_stackCount = (byte) m_spell.InitialStackCount;
      if(m_stackCount > 0 && casterReference.UnitMaster != null)
        m_stackCount =
          casterReference.UnitMaster.Auras.GetModifiedInt(SpellModifierType.Charges, m_spell,
            m_stackCount);
      SetupValues();
    }

    internal Aura(AuraCollection auras, ObjectReference caster, AuraRecord record, List<AuraEffectHandler> handlers,
      byte index)
    {
      m_record = record;
      m_auras = auras;
      m_spell = record.Spell;
      m_beneficial = record.IsBeneficial;
      Id = m_spell.GetAuraUID(m_beneficial);
      m_handlers = handlers;
      m_CasterReference = caster;
      m_index = index;
      m_auraLevel = (byte) record.Level;
      m_stackCount = record.StackCount;
      SetupValues();
      m_duration = record.MillisLeft;
      SetupTimer();
    }

    /// <summary>
    /// Called after setting up the Aura and before calling Start()
    /// </summary>
    private void SetupTimer()
    {
      if(m_controller != null || m_amplitude <= 0 && m_duration <= 0)
        return;
      m_timer = new TimerEntry
      {
        Action = Apply
      };
    }

    private void SetupValues()
    {
      DetermineFlags();
      m_hasPeriodicallyUpdatedEffectHandler = m_handlers.Any(
        handler => handler is PeriodicallyUpdatedAuraEffectHandler);
      if(m_amplitude != 0)
        return;
      foreach(AuraEffectHandler handler in m_handlers)
      {
        if(handler.SpellEffect.Amplitude > 0)
        {
          m_amplitude = handler.SpellEffect.Amplitude;
          break;
        }
      }
    }

    private void DetermineFlags()
    {
      m_auraFlags = m_spell.DefaultAuraFlags;
      if(m_auras.Owner.EntityId == m_CasterReference.EntityId)
        m_auraFlags |= AuraFlags.TargetIsCaster;
      if(m_beneficial)
        m_auraFlags |= AuraFlags.Positive;
      else
        m_auraFlags |= AuraFlags.Negative;
      if(m_spell.Durations.Min > 0)
        m_auraFlags |= AuraFlags.HasDuration;
      for(int index = Math.Min(m_handlers.Count - 1, 2); index >= 0; --index)
      {
        int effectIndex = (int) m_handlers[index].SpellEffect.EffectIndex;
        if(effectIndex >= 0)
          m_auraFlags |= (AuraFlags) (1 << effectIndex);
      }

      if(m_auraFlags != AuraFlags.None)
        return;
      m_auraFlags = AuraFlags.Effect1AppliesAura;
    }

    /// <summary>
    /// The <c>AuraCollection</c> of the Unit that owns this Aura.
    /// </summary>
    public AuraCollection Auras
    {
      get { return m_auras; }
    }

    /// <summary>The Spell that belongs to this Aura</summary>
    public Spell Spell
    {
      get { return m_spell; }
    }

    /// <summary>The amount of times that this Aura has been applied</summary>
    public int StackCount
    {
      get { return m_stackCount; }
      set { m_stackCount = value; }
    }

    /// <summary>Whether this Aura is added to it's owner</summary>
    public bool IsAdded { get; protected internal set; }

    public bool CanBeRemoved
    {
      get
      {
        if(m_spell != null && m_beneficial &&
           !m_spell.AttributesEx.HasAnyFlag(SpellAttributesEx.Negative))
          return !m_spell.Attributes.HasAnyFlag(SpellAttributes.CannotRemove);
        return false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsBeneficial
    {
      get { return m_beneficial; }
    }

    /// <summary>
    /// The amount of ticks left (always 0 for non-periodic auras)
    /// </summary>
    public int TicksLeft
    {
      get { return MaxTicks - Ticks; }
    }

    /// <summary>Information about the caster</summary>
    public ObjectReference CasterReference
    {
      get { return m_CasterReference; }
    }

    /// <summary>
    /// The actual Caster (returns null if caster went offline or disappeared for some other reason)
    /// </summary>
    public Unit CasterUnit
    {
      get
      {
        Unit unitMaster = m_CasterReference.UnitMaster;
        if(unitMaster != null && unitMaster.IsInContext)
          return unitMaster;
        return null;
      }
    }

    /// <summary>
    /// The SpellCast that caused this Aura (if still present)
    /// </summary>
    public SpellCast SpellCast
    {
      get
      {
        SpellChannel controller = Controller as SpellChannel;
        if(controller != null)
          return controller.Cast;
        return CasterUnit?.SpellCast;
      }
    }

    public Unit Owner
    {
      get { return m_auras.Owner; }
    }

    public Item UsedItem
    {
      get
      {
        if(m_UsedItem != null && m_UsedItem.IsInWorld && m_UsedItem.IsInContext)
          return m_UsedItem;
        return null;
      }
      internal set { m_UsedItem = value; }
    }

    /// <summary>
    ///  The amplitude between aura-ticks (only for non-passive auras which are not channeled)
    /// </summary>
    public int Amplitude
    {
      get { return m_amplitude; }
    }

    /// <summary>
    /// Whether this Aura is not visible to the client (only its effects will make him realize it)
    /// </summary>
    public bool IsVisible
    {
      get
      {
        if(m_spell.IsPassive && !m_spell.AttributesEx.HasFlag(SpellAttributesEx.Negative))
          return m_CasterReference.Object != m_auras.Owner;
        return true;
      }
    }

    /// <summary>The maximum amount of Applications for this Aura</summary>
    public int MaxApplications
    {
      get { return m_spell.MaxStackCount; }
    }

    /// <summary>
    /// The position of this Aura within the client's Aura-bar (0 if not exposed to client)
    /// </summary>
    public byte Index
    {
      get { return m_index; }
      set
      {
        RemoveFromClient();
        m_index = value;
        SendToClient();
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
        if(m_controller == null)
          return m_duration - (Environment.TickCount - m_startTime);
        return m_controller.TimeLeft;
      }
      set
      {
        if(!HasTimer)
          return;
        m_startTime = Environment.TickCount;
        int initialDelay;
        if(m_amplitude > 0)
        {
          if(value <= 0)
          {
            m_maxTicks = int.MaxValue;
          }
          else
          {
            m_maxTicks = value / m_amplitude;
            if(m_maxTicks < 1)
              m_maxTicks = 1;
          }

          initialDelay = value % (m_amplitude + 1);
        }
        else
        {
          initialDelay = value;
          m_maxTicks = 1;
        }

        m_ticks = 0;
        if(value < 0)
          m_timer.Stop();
        else
          m_timer.Start(initialDelay);
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
        if(HasTimeout)
          return !m_spell.IsTriggeredSpell;
        return false;
      }
    }

    public IEnumerable<AuraEffectHandler> Handlers
    {
      get { return m_handlers; }
    }

    /// <summary>
    /// The controller of this Aura which controls timing, application and removal (such as <see cref="T:WCell.RealmServer.Spells.SpellChannel">SpellChannels</see>)
    /// </summary>
    public ITickTimer Controller
    {
      get { return m_controller; }
    }

    /// <summary>
    /// Auras that are not passive and not controlled by a <c>ITickTimer</c> have their own Timers
    /// </summary>
    public bool HasTimeout
    {
      get
      {
        if(m_spell.Durations.Min > 0)
          return m_controller == null;
        return false;
      }
    }

    public bool HasTimer
    {
      get { return m_timer != null; }
    }

    public int Ticks
    {
      get
      {
        if(m_controller != null)
          return m_controller.Ticks;
        return m_ticks;
      }
    }

    public int MaxTicks
    {
      get
      {
        if(m_controller != null)
          return m_controller.MaxTicks;
        return m_maxTicks;
      }
    }

    /// <summary>Duration in millis</summary>
    public int Duration
    {
      get
      {
        if(m_controller != null)
          return m_controller.Duration;
        return m_duration;
      }
      set
      {
        m_duration = value;
        m_auraFlags |= AuraFlags.HasDuration;
        SetupTimer();
        TimeLeft = m_duration;
      }
    }

    public int Until
    {
      get
      {
        if(m_spell.IsPassive)
          return -1;
        if(m_controller != null)
          return m_controller.Until;
        return Environment.TickCount - m_startTime;
      }
    }

    public byte Level
    {
      get { return m_auraLevel; }
    }

    public AuraFlags Flags
    {
      get { return m_auraFlags; }
    }

    public bool HasPeriodicallyUpdatedEffectHandler
    {
      get { return m_hasPeriodicallyUpdatedEffectHandler; }
    }

    /// <summary>Method is called</summary>
    /// <param name="noTimeout">Whether the Aura should always continue and never expire.</param>
    public void Start(ITickTimer controller, bool noTimeout)
    {
      m_controller = controller;
      m_duration = !noTimeout ? Spell.GetDuration(m_CasterReference, m_auras.Owner) : -1;
      SetupTimer();
      Start();
    }

    public void Start()
    {
      TimeLeft = m_duration;
      foreach(AuraEffectHandler handler in m_handlers)
        handler.Init(this);
      if(m_auras.MayActivate(this))
        IsActivated = true;
      CanBeSaved = this != m_auras.GhostAura &&
                   !m_spell.AttributesExC.HasFlag(SpellAttributesExC.HonorlessTarget) &&
                   UsedItem == null;
      m_auras.OnAuraChange(this);
      Unit casterUnit = CasterUnit;
      Unit owner = Owner;
    }

    /// <summary>Disables the Aura without removing it's effects</summary>
    public bool IsActivated
    {
      get { return m_IsActivated; }
      set
      {
        if(m_IsActivated == value)
          return;
        if(m_IsActivated = value)
          Activate();
        else
          Deactivate(false);
      }
    }

    private void Activate()
    {
      if(m_spell.IsProc && CasterUnit != null && m_spell.ProcHandlers != null)
      {
        foreach(ProcHandlerTemplate procHandler in m_spell.ProcHandlers)
          Owner.AddProcHandler(new ProcHandler(CasterUnit, Owner, procHandler));
      }

      if(m_spell.IsAuraProcHandler)
        m_auras.Owner.AddProcHandler(this);
      if(m_spell.IsAreaAura && Owner.EntityId == CasterReference.EntityId)
      {
        AreaAura areaAura = m_auras.Owner.GetAreaAura(m_spell);
        if(areaAura != null)
          areaAura.Start(m_controller, !HasTimeout);
      }

      ApplyNonPeriodicEffects();
      SendToClient();
    }

    /// <summary>Called when the Aura gets deactivated</summary>
    /// <param name="cancelled"></param>
    private void Deactivate(bool cancelled)
    {
      if(m_spell.ProcHandlers != null && CasterUnit != null)
      {
        foreach(ProcHandlerTemplate procHandler in m_spell.ProcHandlers)
          Owner.RemoveProcHandler(procHandler);
      }

      if(m_spell.IsAuraProcHandler)
        m_auras.Owner.RemoveProcHandler(this);
      if(m_spell.IsAreaAura && Owner.EntityId == CasterReference.EntityId)
      {
        AreaAura areaAura = m_auras.Owner.GetAreaAura(m_spell);
        if(areaAura != null)
          areaAura.IsActivated = false;
      }

      CallAllHandlers(handler => handler.DoRemove(cancelled));
      RemoveFromClient();
    }

    /// <summary>Applies this Aura's effect to its holder</summary>
    public void Apply()
    {
      Apply(0);
    }

    /// <summary>Applies one of this Aura's Ticks to its holder</summary>
    internal void Apply(int timeElapsed)
    {
      ++m_ticks;
      bool flag = (!m_spell.HasPeriodicAuraEffects || m_ticks >= m_maxTicks) &&
                  m_controller == null;
      if(m_IsActivated)
      {
        OnApply();
        ApplyPeriodicEffects();
        if(!IsAdded)
          return;
        if(!flag && m_timer != null)
          m_timer.Start(m_amplitude);
      }

      if(!flag)
        return;
      Remove(false);
    }

    /// <summary>
    /// Removes and then re-applies all non-perodic Aura-effects
    /// </summary>
    public void ReApplyNonPeriodicEffects()
    {
      RemoveNonPeriodicEffects();
      foreach(AuraEffectHandler handler in m_handlers)
        handler.UpdateEffectValue();
      ApplyNonPeriodicEffects();
    }

    /// <summary>Applies all non-perodic Aura-effects</summary>
    internal void ApplyNonPeriodicEffects()
    {
      if(!m_spell.HasNonPeriodicAuraEffects)
        return;
      foreach(AuraEffectHandler handler in Handlers)
      {
        if(!handler.SpellEffect.IsPeriodic && m_auras.MayActivate(handler))
        {
          handler.DoApply();
          if(!IsAdded)
            break;
        }
      }
    }

    internal void ApplyPeriodicEffects()
    {
      if(!m_spell.HasPeriodicAuraEffects)
        return;
      foreach(AuraEffectHandler handler in m_handlers)
      {
        if(handler.SpellEffect.IsPeriodic && m_auras.MayActivate(handler))
        {
          handler.DoApply();
          if(!IsAdded)
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
      if(!m_spell.IsFood && !m_spell.IsDrink)
        return;
      CasterReference.UnitMaster.Emote(EmoteType.SimpleEat);
    }

    /// <summary>
    /// Refreshes this aura.
    /// If this Aura is stackable, will also increase the StackCount by one.
    /// </summary>
    public void Refresh(ObjectReference caster)
    {
      if(!IsAdded)
        return;
      RemoveNonPeriodicEffects();
      m_CasterReference = caster;
      if(m_spell.InitialStackCount > 1)
        m_stackCount = (byte) m_spell.InitialStackCount;
      else if(m_stackCount < m_spell.MaxStackCount)
        ++m_stackCount;
      foreach(AuraEffectHandler handler in m_handlers)
        handler.UpdateEffectValue();
      ApplyNonPeriodicEffects();
      TimeLeft = m_spell.GetDuration(caster, m_auras.Owner);
      if(!IsVisible)
        return;
      AuraHandler.SendAuraUpdate(m_auras.Owner, this);
    }

    /// <summary>
    /// Checks all handlers and toggles those whose requirements aren't met
    /// </summary>
    internal void ReEvaluateNonPeriodicHandlerRequirements()
    {
      if(!Spell.HasNonPeriodicAuraEffects)
        return;
      foreach(AuraEffectHandler handler in Handlers)
      {
        if(!handler.SpellEffect.IsPeriodic)
          handler.IsActivated = m_auras.MayActivate(handler);
      }
    }

    /// <summary>
    /// Stack or removes the given Aura, if possible.
    /// Returns whether the given incompatible Aura was removed or stacked.
    /// <param name="err">Ok, if stacked or no incompatible Aura was found</param>
    /// </summary>
    public AuraOverrideStatus GetOverrideStatus(ObjectReference caster, Spell spell)
    {
      if(Spell.IsPreventionDebuff)
        return AuraOverrideStatus.Bounced;
      if(Spell == spell)
        return AuraOverrideStatus.Refresh;
      if(caster == CasterReference)
        return spell != Spell ? AuraOverrideStatus.Replace : AuraOverrideStatus.Refresh;
      return !spell.CanOverride(Spell) ? AuraOverrideStatus.Bounced : AuraOverrideStatus.Refresh;
    }

    /// <summary>
    /// Removes and then re-applies all non-perodic Aura-effects
    /// </summary>
    private void RemoveNonPeriodicEffects()
    {
      if(!m_spell.HasNonPeriodicAuraEffects)
        return;
      foreach(AuraEffectHandler handler in m_handlers)
      {
        if(!handler.SpellEffect.IsPeriodic)
          handler.IsActivated = false;
      }
    }

    public bool TryRemove(bool cancelled)
    {
      if(m_spell.IsAreaAura)
      {
        Unit owner = m_auras.Owner;
        if(owner.EntityId.Low != (long) (ulong) CasterReference.EntityId &&
           CasterUnit != null && CasterUnit.UnitMaster != owner)
          return false;
        owner.CancelAreaAura(m_spell);
        return true;
      }

      Remove(cancelled);
      return true;
    }

    public void Cancel()
    {
      Remove(true);
    }

    internal void RemoveWithoutCleanup()
    {
      if(!IsAdded)
        return;
      IsAdded = false;
      Deactivate(true);
      if(m_controller != null)
        m_controller.OnRemove(Owner, this);
      OnRemove();
    }

    /// <summary>Removes this Aura from the player</summary>
    public void Remove(bool cancelled = true)
    {
      if(!IsAdded)
        return;
      IsAdded = false;
      Unit owner = m_auras.Owner;
      if(owner == null)
      {
        LogManager.GetCurrentClassLogger()
          .Warn("Tried to remove Aura {0} but it's owner does not exist anymore.");
      }
      else
      {
        if(m_controller != null)
          m_controller.OnRemove(owner, this);
        AuraCollection auras = m_auras;
        if(CasterUnit != null)
          m_spell.NotifyAuraRemoved(this);
        auras.Remove(this);
        Deactivate(cancelled);
        OnRemove();
        if(!m_spell.IsAreaAura || !(owner.EntityId == CasterReference.EntityId))
          return;
        owner.CancelAreaAura(m_spell);
      }
    }

    private void OnRemove()
    {
      if(m_record == null)
        return;
      m_record.DeleteLater();
      m_record = null;
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
      IsActivated = false;
      if(m_record == null)
        return;
      AuraRecord record = m_record;
      m_record = null;
      record.Recycle();
    }

    /// <summary>See IIAura.OnRemove</summary>
    public void OnRemove(Unit owner, Aura aura)
    {
      throw new NotImplementedException();
    }

    protected internal void SendToClient()
    {
      if(!IsVisible)
        return;
      AuraHandler.SendAuraUpdate(m_auras.Owner, this);
    }

    /// <summary>Removes all of this Aura's occupied fields</summary>
    protected void RemoveFromClient()
    {
      if(!IsVisible)
        return;
      Character owner1 = Owner as Character;
      NPC owner2 = Owner as NPC;
      if(owner2 != null)
        Asda2CombatHandler.SendMonstrStateChangedResponse(owner2, Asda2NpcState.Ok);
      if(owner1 == null)
        return;
      Asda2SpellHandler.SendBuffEndedResponse(owner1, Spell.RealId);
      if(owner1.IsInGroup)
        Asda2GroupHandler.SendPartyMemberBuffInfoResponse(owner1);
      if(owner1.SoulmateCharacter == null)
        return;
      Asda2SoulmateHandler.SendSoulmateBuffUpdateInfoResponse(owner1);
    }

    public void Update(int dt)
    {
      if(m_hasPeriodicallyUpdatedEffectHandler)
      {
        foreach(AuraEffectHandler handler in m_handlers)
        {
          if(handler is PeriodicallyUpdatedAuraEffectHandler)
            ((PeriodicallyUpdatedAuraEffectHandler) handler).Update();
        }
      }

      if(m_timer == null)
        return;
      m_timer.Update(dt);
    }

    public ProcTriggerFlags ProcTriggerFlags
    {
      get { return m_spell.ProcTriggerFlagsProp; }
    }

    public ProcHitFlags ProcHitFlags
    {
      get { return m_spell.ProcHitFlags; }
    }

    /// <summary>Spell to be triggered (if any)</summary>
    public Spell ProcSpell
    {
      get
      {
        if(m_spell.ProcTriggerEffects == null)
          return null;
        return m_spell.ProcTriggerEffects[0].TriggerSpell;
      }
    }

    /// <summary>Chance to proc in %</summary>
    public uint ProcChance
    {
      get
      {
        if(m_spell.ProcChance <= 0U)
          return 100;
        return m_spell.ProcChance;
      }
    }

    public int MinProcDelay
    {
      get { return m_spell.ProcDelay; }
    }

    public DateTime NextProcTime { get; set; }

    public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
    {
      bool flag1 = m_spell.ProcTriggerEffects != null;
      bool flag2 = false;
      if(flag1)
      {
        foreach(AuraEffectHandler handler in m_handlers)
        {
          if(handler.SpellEffect.IsProc && handler.CanProcBeTriggeredBy(action) &&
             handler.SpellEffect.CanProcBeTriggeredBy(action.Spell))
          {
            flag2 = true;
            break;
          }
        }
      }
      else if(action.Spell == null || action.Spell != Spell)
        flag2 = true;

      if(flag2)
        return m_spell.CanProcBeTriggeredBy(m_auras.Owner, action, active);
      return false;
    }

    public void TriggerProc(Unit triggerer, IUnitAction action)
    {
      bool flag = false;
      if(m_spell.ProcTriggerEffects != null)
      {
        foreach(AuraEffectHandler handler in m_handlers)
        {
          if(handler.SpellEffect.IsProc && handler.CanProcBeTriggeredBy(action) &&
             handler.SpellEffect.CanProcBeTriggeredBy(action.Spell))
          {
            handler.OnProc(triggerer, action);
            flag = true;
          }
        }
      }
      else
        flag = true;

      if(!flag || m_spell.ProcCharges <= 0)
        return;
      --m_stackCount;
      if(m_stackCount == 0)
        Remove(false);
      else
        AuraHandler.SendAuraUpdate(m_auras.Owner, this);
    }

    public void Dispose()
    {
      Remove(false);
    }

    public void Save()
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(SaveNow);
    }

    internal void SaveNow()
    {
      if(m_record == null)
      {
        Unit owner = m_auras.Owner;
        if(!(owner is Character))
          throw new InvalidOperationException(string.Format("Tried to save non-Player Aura {0} on: {1}",
            this, owner));
        m_record = AuraRecord.ObtainAuraRecord(this);
      }
      else
        m_record.SyncData(this);

      m_record.Save();
    }

    protected void CallAllHandlers(HandlerDelegate dlgt)
    {
      foreach(AuraEffectHandler handler in m_handlers)
        dlgt(handler);
    }

    public AuraEffectHandler GetHandler(AuraType type)
    {
      foreach(AuraEffectHandler handler in Handlers)
      {
        if(handler.SpellEffect.AuraType == type)
          return handler;
      }

      return null;
    }

    public override string ToString()
    {
      return "Aura " + m_spell + ": " +
             (IsBeneficial ? "Beneficial" : (object) "Harmful") +
             (HasTimeout
               ? " [TimeLeft: " + TimeSpan.FromMilliseconds(TimeLeft) + "]"
               : (object) "") + (m_controller != null
               ? " Controlled by: " + m_controller
               : (object) "");
    }

    public enum AuraOverrideStatus
    {
      NotPresent,
      Replace,
      Refresh,
      Bounced
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
        get { return null; }
      }

      object IEnumerator.Current
      {
        get { return null; }
      }
    }
  }
}