namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModAttackerSpellHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.ModAttackerSpellHitChance(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ModAttackerSpellHitChance(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }
    }
}