namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ModDamageTakenPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.ModDamageTakenPctMod(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ModDamageTakenPctMod(m_spellEffect.MiscBitSet, -EffectValue);
    }
  }
}