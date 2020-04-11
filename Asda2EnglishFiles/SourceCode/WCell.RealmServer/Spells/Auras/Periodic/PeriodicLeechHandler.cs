namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class PeriodicLeechHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.LeechHealth(this.m_aura.CasterUnit, this.EffectValue, this.m_spellEffect.ProcValue,
                this.m_spellEffect);
        }
    }
}