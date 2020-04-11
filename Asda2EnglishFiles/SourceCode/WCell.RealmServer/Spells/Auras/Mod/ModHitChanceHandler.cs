using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.ChangeModifier(StatModifierInt.HitChance, this.SpellEffect.MiscValue);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.ChangeModifier(StatModifierInt.HitChance, -this.SpellEffect.MiscValue);
        }
    }
}