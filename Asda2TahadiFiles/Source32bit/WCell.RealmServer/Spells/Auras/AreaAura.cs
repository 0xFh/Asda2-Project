using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras
{
  /// <summary>
  /// An AreaAura either (if its using an AreaAura spell) applies AreaAura-Effects to everyone in a radius around its center or
  /// triggers its spell on everyone around it.
  /// Can be persistant (Blizzard, Hurricane etc.) or mobile (Paladin Aura)
  /// </summary>
  public class AreaAura : IUpdatable, IAura
  {
    /// <summary>
    /// Delay in milliseconds to wait before revalidating effected targets (Default: 1000 ms).
    /// This is mostly used for Paladin Auras because they don't have an amplitude on their own.
    /// </summary>
    [Variable("DefaultAreaAuraAmplitude")]public static int DefaultAmplitude = 1000;

    private WorldObject m_holder;
    private Spell m_spell;
    private Dictionary<Unit, Aura> m_targets;
    private float m_radius;
    private ITickTimer m_controller;
    private TimerEntry m_timer;
    private ObjectReference m_CasterReference;
    private int m_duration;
    private int m_elapsed;
    private ISpellParameters m_params;
    private int m_remainingCharges;
    private bool m_IsActivated;

    /// <summary>
    /// Creates a new AreaAura that will auto-trigger the given Spell on everyone, according
    /// to the given SpellParameters.
    /// </summary>
    /// <param name="distributedSpell"></param>
    public AreaAura(WorldObject holder, Spell distributedSpell, ISpellParameters prms)
    {
      Init(holder, distributedSpell);
      m_params = prms;
      m_remainingCharges = m_params.MaxCharges;
      m_radius = prms.Radius;
      Start(null, true);
    }

    public AreaAura(WorldObject holder, Spell spell)
    {
      Init(holder, spell);
      m_radius = spell.Effects[0].GetRadius(holder.SharedReference);
    }

    /// <summary>
    /// Creates a new AreaAura which applies its effects to everyone in its radius of influence
    /// </summary>
    protected void Init(WorldObject holder, Spell spell)
    {
      m_holder = holder;
      m_CasterReference =
        !(holder is DynamicObject) ? holder.SharedReference : holder.Master.SharedReference;
      m_spell = spell;
      if(spell.IsAreaAura)
        m_targets = new Dictionary<Unit, Aura>();
      holder.AddAreaAura(this);
    }

    /// <summary>The Holder of this AreaAura.</summary>
    public WorldObject Holder
    {
      get { return m_holder; }
      internal set { m_holder = value; }
    }

    public Spell Spell
    {
      get { return m_spell; }
    }

    /// <summary>
    /// The Position of the holder is also the Center of the Aura.
    /// </summary>
    public Vector3 Center
    {
      get { return m_holder.Position; }
    }

    /// <summary>Radius of the Aura</summary>
    public float Radius
    {
      get { return m_radius; }
      set { m_radius = value; }
    }

    /// <summary>Milliseconds until this expires</summary>
    public int TimeLeft
    {
      get
      {
        if(m_controller == null)
          return m_duration - m_elapsed;
        return m_controller.TimeLeft;
      }
    }

    /// <summary>
    /// Aura is active if its still applied to a <c>Holder</c>
    /// </summary>
    public bool IsAdded
    {
      get { return m_holder != null; }
    }

    /// <summary>
    /// Whether this AreaAura is currently activated and applies it's effects to the area
    /// </summary>
    public bool IsActivated
    {
      get { return m_IsActivated; }
      set
      {
        if(m_IsActivated == value)
          return;
        m_IsActivated = value;
        if(value)
        {
          if(m_timer == null)
            return;
          m_timer.Start();
        }
        else
        {
          if(m_timer != null)
            m_timer.Stop();
          if(m_targets == null)
            return;
          RemoveEffects(m_targets);
          m_targets.Clear();
        }
      }
    }

    /// <summary>Called by a SpellChannel when channeling</summary>
    public void Apply()
    {
      RevalidateTargetsAndApply(0);
    }

    /// <summary>
    /// Initializes this AreaAura with the given controller.
    /// If no controller is given, the AreaAura controls timing and disposal itself.
    /// </summary>
    /// <param name="controller">A controller controls timing and disposal of this AreaAura</param>
    /// <param name="noTimeout">whether the Aura should not expire (ignore the Spell's duration).</param>
    public void Start(ITickTimer controller, bool noTimeout)
    {
      if(m_IsActivated)
        return;
      if(m_radius == 0.0)
        m_radius = 5f;
      m_controller = controller;
      if(m_controller == null || m_controller.MaxTicks == 1)
        m_timer = m_params == null
          ? new TimerEntry(DefaultAmplitude, DefaultAmplitude,
            RevalidateTargetsAndApply)
          : new TimerEntry(m_params.StartDelay,
            m_params.Amplitude != 0 ? m_params.Amplitude : DefaultAmplitude,
            RevalidateTargetsAndApply);
      if(noTimeout)
      {
        m_duration = int.MaxValue;
      }
      else
      {
        m_duration = m_spell.GetDuration(m_CasterReference);
        if(m_duration < 1)
          m_duration = int.MaxValue;
      }

      IsActivated = true;
    }

    public void TryRemove(bool cancelled)
    {
    }

    /// <summary>Remove and dispose AreaAura.</summary>
    public void Remove(bool cancelled)
    {
      IsActivated = false;
      if(m_holder != null)
        m_holder.CancelAreaAura(this);
      m_holder = null;
      m_remainingCharges = 0;
      if(m_timer == null)
        return;
      m_timer.Dispose();
    }

    /// <summary>
    /// Check for all targets in radius, kick out invalid ones and add new ones
    /// </summary>
    protected internal void RevalidateTargetsAndApply(int timeElapsed)
    {
      if(m_controller == null)
      {
        m_elapsed += timeElapsed;
        if(m_elapsed >= m_duration)
        {
          Remove(false);
          return;
        }
      }

      RemoveInvalidTargets();
      bool auraEffects = m_spell.AreaAuraEffects != null;
      List<WorldObject> newTargets = new List<WorldObject>();
      bool exclMobs = m_holder.Faction.Id == FactionId.None;
      m_holder.IterateEnvironment(m_radius, obj =>
      {
        if(obj != m_holder && (exclMobs && obj.IsPlayerOwned || !exclMobs && obj is Unit) &&
           (m_spell.HasHarmfulEffects == m_holder.MayAttack(obj) &&
            m_spell.CheckValidTarget(m_holder, obj) == SpellFailedReason.Ok &&
            (!auraEffects || !m_targets.ContainsKey((Unit) obj))))
          newTargets.Add(obj);
        return true;
      });
      for(int index = 0; index < newTargets.Count; ++index)
      {
        Unit unit = (Unit) newTargets[index];
        if(!IsAdded)
          break;
        if(auraEffects)
          ApplyAuraEffects(unit);
        else
          m_holder.SpellCast.Trigger(m_spell, (WorldObject) unit);
        if(m_holder.IsTrap)
          OnTrapTriggered(unit);
        if(m_remainingCharges != 0)
        {
          --m_remainingCharges;
          if(m_remainingCharges == 0)
            Remove(false);
        }
      }
    }

    /// <summary>
    /// Called when the holder is a trap and the given triggerer triggered it.
    /// </summary>
    /// <param name="triggerer"></param>
    private void OnTrapTriggered(Unit triggerer)
    {
      Unit owner = ((GameObject) m_holder).Owner;
      if(owner == null)
        return;
      Unit unit = triggerer;
      int num1 = 2097152;
      Unit triggerer1 = triggerer;
      TrapTriggerAction trapTriggerAction1 = new TrapTriggerAction();
      trapTriggerAction1.Attacker = owner;
      trapTriggerAction1.Spell = m_spell;
      trapTriggerAction1.Victim = triggerer;
      TrapTriggerAction trapTriggerAction2 = trapTriggerAction1;
      int num2 = 0;
      int num3 = 0;
      unit.Proc((ProcTriggerFlags) num1, triggerer1, trapTriggerAction2, num2 != 0,
        (ProcHitFlags) num3);
    }

    private void RemoveInvalidTargets()
    {
      if(m_targets == null)
        return;
      foreach(KeyValuePair<Unit, Aura> keyValuePair in m_targets
        .Where(target =>
          !target.Key.IsInRadius(m_holder, m_radius)).ToArray())
      {
        if(keyValuePair.Value.IsAdded && keyValuePair.Key.Auras != null)
        {
          if(!keyValuePair.Key.IsInContext && keyValuePair.Key.IsInWorld)
          {
            Aura aura = keyValuePair.Value;
            keyValuePair.Key.AddMessage(() =>
            {
              if(!aura.IsAdded)
                return;
              aura.Remove(false);
            });
          }
          else
            keyValuePair.Value.Remove(false);
        }

        m_targets.Remove(keyValuePair.Key);
      }
    }

    /// <summary>Applies this AreaAura's effects to the given target</summary>
    protected void ApplyAuraEffects(Unit target)
    {
      bool flag = m_spell.IsBeneficialFor(m_CasterReference, target);
      if(SpellCast.CheckDebuffResist(target, m_spell, m_CasterReference.Level, !flag) !=
         CastMissReason.None)
        return;
      Aura aura = target.Auras.CreateAura(m_CasterReference, m_spell, null);
      if(aura == null)
        return;
      aura.Start(m_controller, false);
      m_targets.Add(target, aura);
    }

    /// <summary>Removes all auras from the given targets</summary>
    protected static void RemoveEffects(IEnumerable<KeyValuePair<Unit, Aura>> targets)
    {
      foreach(KeyValuePair<Unit, Aura> target in targets)
        target.Value.Remove(false);
    }

    public void Update(int dt)
    {
      if(m_timer == null)
        return;
      m_timer.Update(dt);
    }
  }
}