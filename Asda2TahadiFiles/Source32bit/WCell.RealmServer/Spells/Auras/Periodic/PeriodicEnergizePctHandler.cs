using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class PeriodicEnergizePctHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if((PowerType) m_spellEffect.MiscValue != Owner.PowerType)
        return;
      m_aura.Auras.Owner.EnergizePercent(EffectValue, m_aura.CasterUnit, m_spellEffect);
    }
  }
}