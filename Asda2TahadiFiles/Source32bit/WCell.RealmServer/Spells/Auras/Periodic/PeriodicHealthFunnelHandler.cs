namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Channeled healing</summary>
  public class PeriodicHealthFunnelHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.Heal(EffectValue, m_aura.CasterUnit, m_spellEffect);
    }
  }
}