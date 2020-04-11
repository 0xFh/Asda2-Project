using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Increases Chance to block</summary>
  public class ModShieldBlockValuePercentHandler : AuraEffectHandler
  {
    private float value;

    protected override void Apply()
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.BlockValue,
        value = EffectValue / 100f);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.BlockValue, -value);
    }
  }
}