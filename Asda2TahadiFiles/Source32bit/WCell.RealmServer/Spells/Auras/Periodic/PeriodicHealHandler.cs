namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class PeriodicHealHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(Owner == null || m_aura == null || m_aura.CasterUnit == null)
        return;
      if(SpellEffect.MiscValueB == 0)
        Owner.Heal(SpellEffect.MiscValue, m_aura.CasterUnit, m_spellEffect);
      if(SpellEffect.MiscValueB != 1)
        return;
      Owner.Heal(
        (int) (SpellEffect.MiscValue * (double) m_aura.CasterUnit.RandomMagicDamage / 100.0),
        m_aura.CasterUnit, m_spellEffect);
    }
  }
}