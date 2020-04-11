using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Drains Mana and applies damage</summary>
  public class PowerBurnHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Unit owner = Owner;
      if(owner.PowerType != (PowerType) m_spellEffect.MiscValue || m_aura.CasterUnit == null)
        return;
      owner.BurnPower(EffectValue, m_spellEffect.ProcValue, m_aura.CasterUnit, m_spellEffect);
    }
  }
}