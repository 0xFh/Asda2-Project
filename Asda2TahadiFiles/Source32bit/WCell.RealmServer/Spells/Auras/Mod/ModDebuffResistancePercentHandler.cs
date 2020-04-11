using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModDebuffResistancePercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ModDebuffResistance((DamageSchool) m_spellEffect.MiscValue, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ModDebuffResistance((DamageSchool) m_spellEffect.MiscValue, -EffectValue);
    }
  }
}