namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModDamageTakenHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.ModDamageTakenMod(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ModDamageTakenMod(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }
    }
}