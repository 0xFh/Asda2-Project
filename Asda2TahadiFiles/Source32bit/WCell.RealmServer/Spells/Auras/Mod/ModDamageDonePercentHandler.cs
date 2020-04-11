namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModDamageDonePercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.ModDamageDoneFactor(m_spellEffect.MiscBitSet, EffectValue / 100f);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ModDamageDoneFactor(m_spellEffect.MiscBitSet, -EffectValue / 100f);
    }
  }
}