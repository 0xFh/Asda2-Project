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
                this.Owner.GetObjectsInRadius<Unit>(6f, ObjectTypes.Unit, false, int.MaxValue);
            if (objectsInRadius == null)
                return;
            foreach (WorldObject worldObject in (IEnumerable<WorldObject>) objectsInRadius)
            {
                Unit unit = worldObject as Unit;
                if (unit != null && this.Owner != null &&
                    (this.m_aura != null && unit.IsHostileWith((IFactionMember) this.Owner)))
                {
                    DamageAction damageAction = unit.DealSpellDamage(this.Owner, this.SpellEffect,
                        (int) ((double) this.Owner.RandomDamage * (double) this.SpellEffect.MiscValue / 100.0), true,
                        true, false, false);
                    if (damageAction != null)
                    {
                        Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(this.m_aura.CasterUnit as Character,
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