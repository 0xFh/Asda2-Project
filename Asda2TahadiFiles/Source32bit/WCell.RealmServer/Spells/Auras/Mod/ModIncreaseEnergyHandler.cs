using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Increase MaxPower</summary>
  public class ModIncreaseEnergyHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.Power, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.Power, -EffectValue);
    }
  }
}