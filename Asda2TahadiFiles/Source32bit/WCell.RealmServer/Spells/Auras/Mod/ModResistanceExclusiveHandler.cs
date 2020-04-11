using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Same as ModResistance?</summary>
  public class ModResistanceExclusiveHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      foreach(DamageSchool miscBit in m_spellEffect.MiscBitSet)
        m_aura.Auras.Owner.AddResistanceBuff(miscBit, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      foreach(DamageSchool miscBit in m_spellEffect.MiscBitSet)
        m_aura.Auras.Owner.RemoveResistanceBuff(miscBit, EffectValue);
    }
  }
}