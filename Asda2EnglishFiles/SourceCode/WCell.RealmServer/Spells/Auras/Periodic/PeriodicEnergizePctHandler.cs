using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class PeriodicEnergizePctHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if ((PowerType) this.m_spellEffect.MiscValue != this.Owner.PowerType)
                return;
            this.m_aura.Auras.Owner.EnergizePercent(this.EffectValue, this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}