using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ModSpellDamageByPercentOfStatHandler : AuraEffectHandler
  {
    private int value;

    protected override void Apply()
    {
      value =
        (Owner.GetTotalStatValue((StatType) SpellEffect.MiscValueB) * EffectValue + 50) / 100;
      Owner.AddDamageDoneMod(m_spellEffect.MiscBitSet, value);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.RemoveDamageDoneMod(m_spellEffect.MiscBitSet, value);
    }
  }
}