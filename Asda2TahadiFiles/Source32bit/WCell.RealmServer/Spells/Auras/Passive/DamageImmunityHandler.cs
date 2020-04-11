namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Same as SchoolImmunityHandler?</summary>
  public class DamageImmunityHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.IncDmgImmunityCount(m_spellEffect);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.DecDmgImmunityCount(m_spellEffect.MiscBitSet);
    }
  }
}