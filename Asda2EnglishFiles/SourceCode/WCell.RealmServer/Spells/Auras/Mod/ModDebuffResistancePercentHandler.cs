using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModDebuffResistancePercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ModDebuffResistance((DamageSchool) this.m_spellEffect.MiscValue, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ModDebuffResistance((DamageSchool) this.m_spellEffect.MiscValue, -this.EffectValue);
        }
    }
}