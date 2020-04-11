using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Mods Melee Attack speed (positive value decreases time)
    /// Same as ModAttackSpeed
    /// </summary>
    public class ModHasteHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, (float) -this.EffectValue / 100f);
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.RangedAttackTime,
                (float) -this.EffectValue / 100f);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, (float) this.EffectValue / 100f);
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.RangedAttackTime, (float) this.EffectValue / 100f);
        }
    }
}