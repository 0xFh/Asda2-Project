using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModMechanicDurationPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ModMechanicDurationMod((SpellMechanic) m_spellEffect.MiscValue,
        EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ModMechanicDurationMod((SpellMechanic) m_spellEffect.MiscValue,
        -EffectValue);
    }
  }
}