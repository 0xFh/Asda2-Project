using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Increases Chance to block</summary>
    public class ModShieldBlockValuePercentHandler : AuraEffectHandler
    {
        private float value;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.BlockValue,
                this.value = (float) this.EffectValue / 100f);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.BlockValue, -this.value);
        }
    }
}