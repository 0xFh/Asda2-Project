using System;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
  /// <summary>Default implementation for IProcHandler</summary>
  public class ProcHandler : IProcHandler, IDisposable
  {
    public static ProcValidator DodgeBlockOrParryValidator = (target, action) =>
    {
      DamageAction damageAction = action as DamageAction;
      if(damageAction == null)
        return false;
      if(damageAction.VictimState != VictimState.Dodge && damageAction.VictimState != VictimState.Parry)
        return damageAction.Blocked > 0;
      return true;
    };

    public static ProcValidator DodgeValidator = (target, action) =>
    {
      DamageAction damageAction = action as DamageAction;
      if(damageAction == null)
        return false;
      return damageAction.VictimState == VictimState.Dodge;
    };

    public static ProcValidator StunValidator = (target, action) =>
    {
      DamageAction damageAction = action as DamageAction;
      if(damageAction == null || damageAction.Spell == null ||
         (!damageAction.Spell.IsAura || !action.Attacker.MayAttack(action.Victim)))
        return false;
      return damageAction.Spell.Attributes.HasAnyFlag(SpellAttributes.MovementImpairing);
    };

    public readonly WeakReference<Unit> CreatorRef;
    public readonly ProcHandlerTemplate Template;
    private int m_stackCount;

    public ProcHandler(Unit creator, Unit owner, ProcHandlerTemplate template)
    {
      CreatorRef = new WeakReference<Unit>(creator);
      Owner = owner;
      Template = template;
      m_stackCount = template.StackCount;
    }

    public Unit Owner { get; private set; }

    /// <summary>The amount of times that this Aura has been applied</summary>
    public int StackCount
    {
      get { return m_stackCount; }
      set { m_stackCount = value; }
    }

    public ProcTriggerFlags ProcTriggerFlags
    {
      get { return Template.ProcTriggerFlags; }
    }

    public ProcHitFlags ProcHitFlags
    {
      get { return Template.ProcHitFlags; }
    }

    public Spell ProcSpell
    {
      get { return null; }
    }

    /// <summary>Chance to proc in %</summary>
    public uint ProcChance
    {
      get { return Template.ProcChance; }
    }

    public int MinProcDelay
    {
      get { return Template.MinProcDelay; }
    }

    public DateTime NextProcTime { get; set; }

    /// <param name="active">Whether the triggerer is the attacker/caster (true), or the victim (false)</param>
    public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
    {
      if(Template.Validator != null)
        return Template.Validator(triggerer, action);
      return true;
    }

    public void TriggerProc(Unit triggerer, IUnitAction action)
    {
      if(!CreatorRef.IsAlive)
      {
        Dispose();
      }
      else
      {
        if(!Template.ProcAction(CreatorRef, triggerer, action) || m_stackCount <= 0)
          return;
        --m_stackCount;
      }
    }

    public void Dispose()
    {
      Owner.RemoveProcHandler(this);
    }
  }
}