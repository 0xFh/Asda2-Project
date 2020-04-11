namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class PeriodicLeechHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.LeechHealth(m_aura.CasterUnit, EffectValue, m_spellEffect.ProcValue,
        m_spellEffect);
    }
  }
}