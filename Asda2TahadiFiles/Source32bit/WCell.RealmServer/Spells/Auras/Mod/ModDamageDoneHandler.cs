namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModDamageDoneHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.AddDamageDoneMod(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.RemoveDamageDoneMod(m_spellEffect.MiscBitSet, EffectValue);
    }
  }
}