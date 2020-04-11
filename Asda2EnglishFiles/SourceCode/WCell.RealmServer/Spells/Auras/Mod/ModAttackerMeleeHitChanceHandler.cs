using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModAttackerMeleeHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.ChangeModifier(StatModifierInt.AttackerMeleeHitChance, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ChangeModifier(StatModifierInt.AttackerMeleeHitChance, -this.EffectValue);
        }
    }
}