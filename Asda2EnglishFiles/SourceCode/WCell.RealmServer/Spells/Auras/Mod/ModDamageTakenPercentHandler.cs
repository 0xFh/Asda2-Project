namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModDamageTakenPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.ModDamageTakenPctMod(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ModDamageTakenPctMod(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }
    }
}