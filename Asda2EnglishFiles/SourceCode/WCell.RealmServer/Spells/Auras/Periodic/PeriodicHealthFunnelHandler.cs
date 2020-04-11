namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Channeled healing</summary>
    public class PeriodicHealthFunnelHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.Heal(this.EffectValue, this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}