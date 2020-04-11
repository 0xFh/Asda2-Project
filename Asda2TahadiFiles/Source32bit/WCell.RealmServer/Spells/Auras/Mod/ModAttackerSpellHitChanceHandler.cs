namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ModAttackerSpellHitChanceHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.ModAttackerSpellHitChance(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ModAttackerSpellHitChance(m_spellEffect.MiscBitSet, -EffectValue);
    }
  }
}