namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModDamageDonePercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.ModDamageDoneFactor(this.m_spellEffect.MiscBitSet, (float) this.EffectValue / 100f);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ModDamageDoneFactor(this.m_spellEffect.MiscBitSet, (float) -this.EffectValue / 100f);
        }
    }
}