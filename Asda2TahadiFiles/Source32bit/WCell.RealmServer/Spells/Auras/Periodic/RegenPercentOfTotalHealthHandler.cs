using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Regenerates a percentage of your total Mana every tick
  /// </summary>
  public class RegenPercentOfTotalHealthHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Unit owner = m_aura.Auras.Owner;
      if(!owner.IsAlive)
        return;
      owner.HealPercent(EffectValue, m_aura.CasterUnit, m_spellEffect);
    }
  }
}