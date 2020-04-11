namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Same as SchoolImmunityHandler?</summary>
    public class DamageImmunityHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.IncDmgImmunityCount(this.m_spellEffect);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.DecDmgImmunityCount(this.m_spellEffect.MiscBitSet);
        }
    }
}