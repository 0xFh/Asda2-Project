using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Haste for melee, ranged and spells in %</summary>
    public class ModHastePercentHandler : AuraEffectHandler
    {
        private float val;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.MeleeAttackTime,
                this.val = (float) -this.EffectValue / 100f);
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.RangedAttackTime,
                this.val = (float) -this.EffectValue / 100f);
            this.m_aura.Auras.Owner.CastSpeedFactor += this.val;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, -this.val);
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.RangedAttackTime, -this.val);
            this.m_aura.Auras.Owner.CastSpeedFactor -= this.val;
        }
    }
}