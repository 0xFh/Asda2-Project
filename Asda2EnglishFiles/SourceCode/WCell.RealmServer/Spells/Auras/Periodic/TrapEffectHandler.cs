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
                this.Owner.GetObjectsInRadius<Unit>(3f, ObjectTypes.Unit, false, int.MaxValue);
            bool flag = false;
            foreach (WorldObject worldObject in (IEnumerable<WorldObject>) objectsInRadius)
            {
                Unit unit = worldObject as Unit;
                if (unit != null && unit.IsHostileWith((IFactionMember) this.Owner))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
                return;
            foreach (WorldObject objectsInRadiu in (IEnumerable<WorldObject>) this.Owner.GetObjectsInRadius<Unit>(12f,
                ObjectTypes.Unit, false, int.MaxValue))
            {
                Unit pos = objectsInRadiu as Unit;
                if (pos != null && pos.IsHostileWith((IFactionMember) this.Owner))
                {
                    if (this.SpellEffect.MiscValueB == 1)
                    {
                        Spell spell = SpellHandler.Get(775U);
                        pos.Auras.CreateAndStartAura(this.Owner.SharedReference, spell, false, (Item) null);
                    }
                    else if (this.SpellEffect.MiscValueB == 0)
                    {
                        float dist = pos.GetDist((IHasPosition) this.Owner);
                        float num = 1f;
                        if ((double) dist >= 3.0)
                            num /= (float) Math.Pow((double) dist, 0.600000023841858);
                        DamageAction damageAction = pos.DealSpellDamage(this.Owner, this.SpellEffect,
                            (int) ((double) this.Owner.RandomDamage * (double) this.SpellEffect.MiscValue / 100.0 *
                                   (double) num), true, true, false, false);
                        if (damageAction != null)
                        {
                            if (this.m_aura != null)
                                Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(
                                    this.m_aura.CasterUnit as Character, objectsInRadiu as Character,
                                    objectsInRadiu as NPC, damageAction.ActualDamage);
                            damageAction.OnFinished();
                        }
                    }
                }
            }

            this.Aura.Cancel();
        }

        protected override void Remove(bool cancelled)
        {
        }
    }
}