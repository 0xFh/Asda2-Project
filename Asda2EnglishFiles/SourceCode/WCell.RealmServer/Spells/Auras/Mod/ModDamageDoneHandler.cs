namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModDamageDoneHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.AddDamageDoneMod(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.RemoveDamageDoneMod(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }
    }
}