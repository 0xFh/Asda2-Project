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
            Unit casterUnit = this.m_aura.CasterUnit;
            if (casterUnit == null)
                return;
            foreach (WorldObject objectsInRadiu in (IEnumerable<WorldObject>) this.Owner.GetObjectsInRadius<Unit>(6f,
                ObjectTypes.Unit, false, int.MaxValue))
            {
                if (casterUnit.IsHostileWith((IFactionMember) objectsInRadiu))
                {
                    Unit unit = objectsInRadiu as Unit;
                    if (unit != null)
                    {
                        DamageAction damageAction = unit.DealSpellDamage(casterUnit, this.SpellEffect,
                            (int) ((double) casterUnit.RandomDamage * (double) this.SpellEffect.MiscValue / 100.0),
                            true, true, false, false);
                        if (damageAction != null)
                        {
                            Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(
                                this.m_aura.CasterUnit as Character, this.Owner as Character, unit as NPC,
                                damageAction.ActualDamage);
                            damageAction.OnFinished();
                        }
                    }
                }
            }
        }
    }
}