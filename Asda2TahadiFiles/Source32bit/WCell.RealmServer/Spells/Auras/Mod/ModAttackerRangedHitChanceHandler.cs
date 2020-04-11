using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ModAttackerRangedHitChanceHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.ChangeModifier(StatModifierInt.AttackerRangedHitChance, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ChangeModifier(StatModifierInt.AttackerRangedHitChance, -EffectValue);
    }
  }
}