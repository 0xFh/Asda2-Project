using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Gives a chance of $s1% of all melee and ranged attacks to land on the Caster instead of the Aura-owner
    /// </summary>
    public class AddCasterHitTriggerHandler : AttackEventEffectHandler
    {
        protected override void Apply()
        {
            if (this.SpellEffect.Spell.RealId == (short) 160)
            {
                switch (this.SpellEffect.Spell.Level)
                {
                    case 1:
                        this.m_aura.Owner.SplinterEffect = 0.1f;
                        this.m_aura.Owner.SplinterEffectChange = 15000;
                        break;
                    case 2:
                        this.m_aura.Owner.SplinterEffect = 0.15f;
                        this.m_aura.Owner.SplinterEffectChange = 20000;
                        break;
                    case 3:
                        this.m_aura.Owner.SplinterEffect = 0.21f;
                        this.m_aura.Owner.SplinterEffectChange = 25000;
                        break;
                    case 4:
                        this.m_aura.Owner.SplinterEffect = 0.28f;
                        this.m_aura.Owner.SplinterEffectChange = 30000;
                        break;
                    case 5:
                        this.m_aura.Owner.SplinterEffect = 0.38f;
                        this.m_aura.Owner.SplinterEffectChange = 35000;
                        break;
                    case 6:
                        this.m_aura.Owner.SplinterEffect = 0.5f;
                        this.m_aura.Owner.SplinterEffectChange = 40000;
                        break;
                    case 7:
                        this.m_aura.Owner.SplinterEffect = 0.65f;
                        this.m_aura.Owner.SplinterEffectChange = 45000;
                        break;
                }
            }

            base.Apply();
        }

        protected override void Remove(bool cancelled)
        {
            if (this.SpellEffect.Spell.RealId == (short) 160)
                this.m_aura.Owner.SplinterEffect = 0.0f;
            base.Remove(cancelled);
        }

        public override void OnAttack(DamageAction action)
        {
            if (action.Spell != null)
                return;
            if ((double) this.Owner.SplinterEffect > 0.0)
            {
                foreach (WorldObject objectsInRadiu in (IEnumerable<WorldObject>) this.Owner.GetObjectsInRadius<Unit>(
                    2.5f, ObjectTypes.Attackable, false, int.MaxValue))
                {
                    if (this.Owner.IsHostileWith((IFactionMember) objectsInRadiu) &&
                        !object.ReferenceEquals((object) objectsInRadiu, (object) this.Owner) &&
                        Utility.Random(0, 100000) <= this.Owner.SplinterEffectChange)
                    {
                        Character targetChr = objectsInRadiu as Character;
                        NPC targetNpc = objectsInRadiu as NPC;
                        DamageAction unusedAction = this.Owner.GetUnusedAction();
                        unusedAction.Damage =
                            (int) ((double) this.Owner.RandomDamage * (double) this.Owner.SplinterEffect);
                        unusedAction.Attacker = objectsInRadiu as Unit;
                        unusedAction.Victim = objectsInRadiu as Unit;
                        int num = (int) unusedAction.DoAttack();
                        if (this.Owner is Character)
                            Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse((Character) null, targetChr,
                                targetNpc, unusedAction.ActualDamage);
                        action.OnFinished();
                    }
                }
            }

            base.OnAttack(action);
        }
    }
}