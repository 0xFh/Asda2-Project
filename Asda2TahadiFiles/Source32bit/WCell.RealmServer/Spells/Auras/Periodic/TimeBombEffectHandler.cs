using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Periodic
{
  public class TimeBombEffectHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
    }

    protected override void Remove(bool cancelled)
    {
      Unit casterUnit = m_aura.CasterUnit;
      if(casterUnit == null)
        return;
      foreach(WorldObject objectsInRadiu in Owner.GetObjectsInRadius(6f,
        ObjectTypes.Unit, false, int.MaxValue))
      {
        if(casterUnit.IsHostileWith(objectsInRadiu))
        {
          Unit unit = objectsInRadiu as Unit;
          if(unit != null)
          {
            DamageAction damageAction = unit.DealSpellDamage(casterUnit, SpellEffect,
              (int) (casterUnit.RandomDamage * (double) SpellEffect.MiscValue / 100.0),
              true, true, false, false);
            if(damageAction != null)
            {
              Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(
                m_aura.CasterUnit as Character, Owner as Character, unit as NPC,
                damageAction.ActualDamage);
              damageAction.OnFinished();
            }
          }
        }
      }
    }
  }
}