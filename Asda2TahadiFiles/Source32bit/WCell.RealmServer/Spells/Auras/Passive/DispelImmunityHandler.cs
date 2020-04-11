using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Adds immunity against a specific DispelType</summary>
  public class DispelImmunityHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.IncDispelImmunity((DispelType) m_spellEffect.MiscValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.DecDispelImmunity((DispelType) m_spellEffect.MiscValue);
    }
  }
}