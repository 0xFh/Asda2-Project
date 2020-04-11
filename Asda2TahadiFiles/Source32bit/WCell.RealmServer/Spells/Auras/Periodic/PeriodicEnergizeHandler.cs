using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class PeriodicEnergizeHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if((PowerType) m_spellEffect.MiscValue != m_aura.Auras.Owner.PowerType)
        return;
      m_aura.Auras.Owner.Energize(EffectValue, m_aura.CasterUnit, m_spellEffect);
    }
  }
}