using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ManaShieldHandler : AttackEventEffectHandler
    {
        private float factor;
        private float factorInverse;
        private int remaining;

        protected override void Apply()
        {
            this.factor = (double) this.SpellEffect.ProcValue != 0.0 ? this.SpellEffect.ProcValue : 1f;
            this.factorInverse = 1f / this.factor;
            this.remaining = this.EffectValue;
            base.Apply();
        }

        public override void OnDefend(DamageAction action)
        {
            Unit owner = this.Owner;
            int power = owner.Power;
            int damage = action.Damage;
            int num1 = Math.Min(damage, (int) ((double) power * (double) this.factorInverse));
            if (this.remaining < num1)
            {
                num1 = this.remaining;
                this.remaining = 0;
                this.m_aura.Remove(false);
            }
            else
                this.remaining -= num1;

            int num2 = (int) ((double) num1 * (double) this.factor);
            Unit casterUnit = this.Aura.CasterUnit;
            if (casterUnit != null)
                num2 = casterUnit.Auras.GetModifiedInt(SpellModifierType.HealingOrPowerGain, this.m_spellEffect.Spell,
                    num2);
            owner.Power = power - num2;
            int num3 = damage - num2;
            action.Damage = num3;
        }
    }
}