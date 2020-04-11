using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModPowerRegenPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (this.m_aura.Auras.Owner.PowerType != (PowerType) this.m_spellEffect.MiscValue)
                return;
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerRegenPercent, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            if (this.m_aura.Auras.Owner.PowerType != (PowerType) this.m_spellEffect.MiscValue)
                return;
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerRegenPercent, -this.EffectValue);
        }
    }
}