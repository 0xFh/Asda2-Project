namespace WCell.RealmServer.Spells.Auras.Misc
{
  public class PhaseAuraHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.Phase = (uint) m_spellEffect.MiscValue;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.Phase = 1U;
    }
  }
}