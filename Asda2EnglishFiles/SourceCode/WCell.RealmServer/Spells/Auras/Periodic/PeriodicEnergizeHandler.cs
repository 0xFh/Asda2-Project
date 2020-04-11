using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class PeriodicEnergizeHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if ((PowerType) this.m_spellEffect.MiscValue != this.m_aura.Auras.Owner.PowerType)
                return;
            this.m_aura.Auras.Owner.Energize(this.EffectValue, this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}