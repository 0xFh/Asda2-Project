using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModPowerRegenHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
            Unit target, ref SpellFailedReason failReason)
        {
            PowerType miscValue = (PowerType) this.m_spellEffect.MiscValue;
            if (target.PowerType == miscValue)
                return;
            failReason = SpellFailedReason.BadTargets;
        }

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerRegen, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerRegen, -this.EffectValue);
        }
    }
}