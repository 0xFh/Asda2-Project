using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ExplosiveArrowEffectHandler : AttackEventEffectHandler
  {
    public override void OnBeforeAttack(DamageAction action)
    {
    }

    public override void OnAttack(DamageAction action)
    {
      if(action.Spell == null || action.SpellEffect.AuraType == AuraType.ExplosiveArrow)
        return;
      foreach(WorldObject objectsInRadiu in action.Victim.GetObjectsInRadius(6f,
        ObjectTypes.Unit, false, int.MaxValue))
      {
        if(objectsInRadiu.IsHostileWith(action.Attacker))
        {
          Unit unit = objectsInRadiu as Unit;
          if(unit != null)
          {
            DamageAction damageAction = unit.DealSpellDamage(action.Attacker, SpellEffect,
              (int) (action.Attacker.RandomDamage * (double) SpellEffect.MiscValue / 100.0),
              true, true, false, false);
            if(damageAction != null)
            {
              Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(
                m_aura.CasterUnit as Character, Owner as Character, unit as NPC,
                damageAction.ActualDamage);
              action.OnFinished();
            }
          }
        }
      }
    }
  }
}