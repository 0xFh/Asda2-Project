using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ModAttackerMeleeHitChanceHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.ChangeModifier(StatModifierInt.AttackerMeleeHitChance, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ChangeModifier(StatModifierInt.AttackerMeleeHitChance, -EffectValue);
    }
  }
}