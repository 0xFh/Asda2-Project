using System;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Periodic
{
  public class TrapEffectHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      IList<WorldObject> objectsInRadius =
        Owner.GetObjectsInRadius(3f, ObjectTypes.Unit, false, int.MaxValue);
      bool flag = false;
      foreach(WorldObject worldObject in objectsInRadius)
      {
        Unit unit = worldObject as Unit;
        if(unit != null && unit.IsHostileWith(Owner))
        {
          flag = true;
          break;
        }
      }

      if(!flag)
        return;
      foreach(WorldObject objectsInRadiu in Owner.GetObjectsInRadius(12f,
        ObjectTypes.Unit, false, int.MaxValue))
      {
        Unit pos = objectsInRadiu as Unit;
        if(pos != null && pos.IsHostileWith(Owner))
        {
          if(SpellEffect.MiscValueB == 1)
          {
            Spell spell = SpellHandler.Get(775U);
            pos.Auras.CreateAndStartAura(Owner.SharedReference, spell, false, null);
          }
          else if(SpellEffect.MiscValueB == 0)
          {
            float dist = pos.GetDist(Owner);
            float num = 1f;
            if(dist >= 3.0)
              num /= (float) Math.Pow(dist, 0.600000023841858);
            DamageAction damageAction = pos.DealSpellDamage(Owner, SpellEffect,
              (int) (Owner.RandomDamage * (double) SpellEffect.MiscValue / 100.0 *
                     num), true, true, false, false);
            if(damageAction != null)
            {
              if(m_aura != null)
                Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(
                  m_aura.CasterUnit as Character, objectsInRadiu as Character,
                  objectsInRadiu as NPC, damageAction.ActualDamage);
              damageAction.OnFinished();
            }
          }
        }
      }

      Aura.Cancel();
    }

    protected override void Remove(bool cancelled)
    {
    }
  }
}