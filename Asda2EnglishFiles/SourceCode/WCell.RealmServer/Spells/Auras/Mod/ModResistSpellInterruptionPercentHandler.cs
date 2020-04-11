namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Increases the resistance against Spell Interruption</summary>
    public class ModResistSpellInterruptionPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ModSpellInterruptProt(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ModSpellInterruptProt(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }
    }
}