using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>TODO: Needs to be value in %</summary>
  public class ModResistancePctHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      for(int index = 0; index < m_spellEffect.MiscBitSet.Length; ++index)
        m_aura.Auras.Owner.AddResistanceBuff((DamageSchool) m_spellEffect.MiscBitSet[index],
          EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      for(int index = 0; index < m_spellEffect.MiscBitSet.Length; ++index)
        m_aura.Auras.Owner.RemoveResistanceBuff((DamageSchool) m_spellEffect.MiscBitSet[index],
          EffectValue);
    }
  }
}