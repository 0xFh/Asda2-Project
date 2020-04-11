using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>TODO: Reapply when AP changes</summary>
  public class ModSpellPowerByAPPctHandler : AuraEffectHandler
  {
    private int[] values;

    protected override void Apply()
    {
      Unit owner = Owner;
      values = new int[m_spellEffect.MiscBitSet.Length];
      for(int index = 0; index < m_spellEffect.MiscBitSet.Length; ++index)
      {
        uint miscBit = m_spellEffect.MiscBitSet[index];
        int delta = (owner.GetDamageDoneMod((DamageSchool) miscBit) * EffectValue + 50) / 100;
        values[index] = delta;
        owner.AddDamageDoneModSilently((DamageSchool) miscBit, delta);
      }
    }

    protected override void Remove(bool cancelled)
    {
      Unit owner = Owner;
      for(int index = 0; index < m_spellEffect.MiscBitSet.Length; ++index)
      {
        uint miscBit = m_spellEffect.MiscBitSet[index];
        owner.RemoveDamageDoneModSilently((DamageSchool) miscBit, values[index]);
      }
    }
  }
}