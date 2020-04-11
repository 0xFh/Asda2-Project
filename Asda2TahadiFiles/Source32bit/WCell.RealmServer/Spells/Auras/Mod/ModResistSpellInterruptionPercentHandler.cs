namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Increases the resistance against Spell Interruption</summary>
  public class ModResistSpellInterruptionPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ModSpellInterruptProt(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ModSpellInterruptProt(m_spellEffect.MiscBitSet, -EffectValue);
    }
  }
}