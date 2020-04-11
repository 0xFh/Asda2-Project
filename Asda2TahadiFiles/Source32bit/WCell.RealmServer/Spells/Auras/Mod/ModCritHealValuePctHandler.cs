using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModCritHealValuePctHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.ChangeModifier(StatModifierInt.CriticalHealValuePct, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ChangeModifier(StatModifierInt.CriticalHealValuePct, -EffectValue);
    }
  }
}