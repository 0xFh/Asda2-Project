using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModCritHealValuePctHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.ChangeModifier(StatModifierInt.CriticalHealValuePct, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ChangeModifier(StatModifierInt.CriticalHealValuePct, -this.EffectValue);
        }
    }
}