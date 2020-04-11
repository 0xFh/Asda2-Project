using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModStatHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.AddStatMod((StatType) m_spellEffect.MiscValue, EffectValue,
        m_aura.Spell.IsPassive);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.RemoveStatMod((StatType) m_spellEffect.MiscValue, EffectValue,
        m_aura.Spell.IsPassive);
    }
  }
}