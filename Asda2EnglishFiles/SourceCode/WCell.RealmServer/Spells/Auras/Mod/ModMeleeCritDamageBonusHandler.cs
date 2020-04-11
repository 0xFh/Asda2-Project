using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>Increases crit damage in %</summary>
    public class ModMeleeCritDamageBonusHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.ChangeModifier(StatModifierInt.CritDamageBonusPct, this.SpellEffect.MiscValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ChangeModifier(StatModifierInt.CritDamageBonusPct, -this.SpellEffect.MiscValue);
        }
    }
}