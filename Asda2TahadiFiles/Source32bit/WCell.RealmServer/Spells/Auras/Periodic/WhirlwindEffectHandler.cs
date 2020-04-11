using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Periodic
{
  /// <summary>Periodically damages the holder</summary>
  public class WhirlwindEffectHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      IList<WorldObject> objectsInRadius =
        Owner.GetObjectsInRadius(6f, ObjectTypes.Unit, false, int.MaxValue);
      if(objectsInRadius == null)
        return;
      foreach(WorldObject worldObject in objectsInRadius)
      {
        Unit unit = worldObject as Unit;
        if(unit != null && Owner != null &&
           (m_aura != null && unit.IsHostileWith(Owner)))
        {
          DamageAction damageAction = unit.DealSpellDamage(Owner, SpellEffect,
            (int) (Owner.RandomDamage * (double) SpellEffect.MiscValue / 100.0), true,
            true, false, false);
          if(damageAction != null)
          {
            Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character,
              unit as Character, unit as NPC, damageAction.ActualDamage);
            damageAction.OnFinished();
          }
        }
      }
    }

    protected override void Remove(bool cancelled)
    {
    }
  }
}