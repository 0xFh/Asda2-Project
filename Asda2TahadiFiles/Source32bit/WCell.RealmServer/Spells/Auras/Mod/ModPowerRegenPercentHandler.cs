using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModPowerRegenPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(m_aura.Auras.Owner.PowerType != (PowerType) m_spellEffect.MiscValue)
        return;
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerRegenPercent, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      if(m_aura.Auras.Owner.PowerType != (PowerType) m_spellEffect.MiscValue)
        return;
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerRegenPercent, -EffectValue);
    }
  }
}