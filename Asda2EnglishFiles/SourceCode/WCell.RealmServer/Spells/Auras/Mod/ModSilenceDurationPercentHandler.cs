using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// 
    /// </summary>
    public class ModSilenceDurationPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ModMechanicDurationMod((SpellMechanic) this.m_spellEffect.MiscValue,
                this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ModMechanicDurationMod((SpellMechanic) this.m_spellEffect.MiscValue,
                -this.EffectValue);
        }
    }
}