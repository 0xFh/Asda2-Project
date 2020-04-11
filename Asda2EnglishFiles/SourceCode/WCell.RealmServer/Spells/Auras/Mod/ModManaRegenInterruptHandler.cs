using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Boosts interrupted Mana regen.
    /// See: http://www.wowwiki.com/Formulas:Mana_Regen#Five_Second_Rule
    /// </summary>
    public class ModManaRegenInterruptHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierInt.ManaRegenInterruptPct, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierInt.ManaRegenInterruptPct, -this.EffectValue);
        }
    }
}