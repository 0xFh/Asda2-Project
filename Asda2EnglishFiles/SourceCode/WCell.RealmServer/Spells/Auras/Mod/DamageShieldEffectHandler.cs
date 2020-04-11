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
            if (this.SpellEffect.MiscValueB == 100)
            {
                foreach (WorldObject objectsInRadiu in (IEnumerable<WorldObject>) this.Owner.GetObjectsInRadius<Unit>(
                    8f, ObjectTypes.Unit, false, int.MaxValue))
                {
                    Unit unit = objectsInRadiu as Unit;
                    if (unit != null && unit.IsHostileWith((IFactionMember) this.Owner))
                    {
                        Spell spell = SpellHandler.Get(74U);
                        spell.Duration = 6000;
                        unit.Auras.CreateAndStartAura(this.Owner.SharedReference, spell, false, (Item) null);
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
            action.Victim.AddMessage((Action) (() =>
            {
                if (!action.Victim.MayAttack((IFactionMember) action.Attacker))
                    return;
                action.Attacker.DealSpellDamage(action.Victim, this.SpellEffect,
                    action.Damage * (this.SpellEffect.MiscValue / 100), true, true, false, true);
                Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(this.m_aura.CasterUnit as Character,
                    this.Owner as Character, this.Owner as NPC, action.ActualDamage);
            }));
            if (this.SpellEffect.MiscValueB == 0)
            {
                action.Resisted = action.Damage;
            }
            else
            {
                if (this.SpellEffect.MiscValueB != 20)
                    return;
                Character casterUnit = this.m_aura.CasterUnit as Character;
                if (casterUnit == null || !casterUnit.IsInGroup)
                    return;
                foreach (GroupMember groupMember in casterUnit.Group)
                    groupMember.Character.Heal(action.Damage * this.SpellEffect.MiscValue / 100, (Unit) null,
                        (SpellEffect) null);
            }
        }
    }
}