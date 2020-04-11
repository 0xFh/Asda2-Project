using System;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>Do flat damage to any attacker</summary>
  public class DamageShieldEffectHandler : AttackEventEffectHandler
  {
    protected override void Apply()
    {
      if(SpellEffect.MiscValueB == 100)
      {
        foreach(WorldObject objectsInRadiu in Owner.GetObjectsInRadius(
          8f, ObjectTypes.Unit, false, int.MaxValue))
        {
          Unit unit = objectsInRadiu as Unit;
          if(unit != null && unit.IsHostileWith(Owner))
          {
            Spell spell = SpellHandler.Get(74U);
            spell.Duration = 6000;
            unit.Auras.CreateAndStartAura(Owner.SharedReference, spell, false, null);
            spell.Duration = 3000;
          }
        }
      }

      base.Apply();
    }

    public override void OnBeforeAttack(DamageAction action)
    {
    }

    public override void OnAttack(DamageAction action)
    {
    }

    public override void OnDefend(DamageAction action)
    {
      action.Victim.AddMessage(() =>
      {
        if(!action.Victim.MayAttack(action.Attacker))
          return;
        action.Attacker.DealSpellDamage(action.Victim, SpellEffect,
          action.Damage * (SpellEffect.MiscValue / 100), true, true, false, true);
        Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character,
          Owner as Character, Owner as NPC, action.ActualDamage);
      });
      if(SpellEffect.MiscValueB == 0)
      {
        action.Resisted = action.Damage;
      }
      else
      {
        if(SpellEffect.MiscValueB != 20)
          return;
        Character casterUnit = m_aura.CasterUnit as Character;
        if(casterUnit == null || !casterUnit.IsInGroup)
          return;
        foreach(GroupMember groupMember in casterUnit.Group)
          groupMember.Character.Heal(action.Damage * SpellEffect.MiscValue / 100, null,
            null);
      }
    }
  }
}