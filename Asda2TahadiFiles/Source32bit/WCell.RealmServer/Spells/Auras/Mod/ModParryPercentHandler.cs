using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Increases Chance to block</summary>
  public class ModParryPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.ParryChance, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.ParryChance, -EffectValue);
    }
  }
}