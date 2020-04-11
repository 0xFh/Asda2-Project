using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ModHealingByPercentOfStatHandler : AuraEffectHandler
  {
    private int value;

    protected override void Apply()
    {
      if(!(m_aura.Auras.Owner is Character))
        return;
      value =
        (Owner.GetTotalStatValue((StatType) SpellEffect.MiscValueB) * EffectValue + 50) / 100;
      ((Character) m_aura.Auras.Owner).HealingDoneMod += value;
    }

    protected override void Remove(bool cancelled)
    {
      if(!(m_aura.Auras.Owner is Character))
        return;
      ((Character) m_aura.Auras.Owner).HealingDoneMod -= value;
    }
  }
}