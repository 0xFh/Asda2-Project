using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModMeleeAttackPowerByPercentOfStatHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.ModMeleeAPModByStat((StatType) m_spellEffect.MiscValue, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.ModMeleeAPModByStat((StatType) m_spellEffect.MiscValue, -EffectValue);
    }
  }
}