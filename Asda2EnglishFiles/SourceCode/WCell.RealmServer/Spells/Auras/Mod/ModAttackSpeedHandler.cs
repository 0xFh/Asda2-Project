using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Adds AttackSpeed speed in %</summary>
    public class ModAttackSpeedHandler : AuraEffectHandler
    {
        private float val;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.MeleeAttackTime,
                -(this.val = (float) this.EffectValue / 100f));
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, this.val);
        }
    }
}