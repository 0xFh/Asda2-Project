using System;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class PeriodicManaLeechHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
            Unit target, ref SpellFailedReason failReason)
        {
            if (target.MaxPower != 0 && target.PowerType == (PowerType) this.m_spellEffect.MiscValue)
                return;
            failReason = SpellFailedReason.BadTargets;
        }

        protected override void Apply()
        {
            int amount = this.EffectValue;
            Unit owner = this.m_aura.Auras.Owner;
            if (this.m_aura.Spell.HasEffectWith((Predicate<SpellEffect>) (effect => effect.AuraType == AuraType.Dummy)))
                amount = owner.BasePower * amount / 100;
            owner.LeechPower(amount, 1f, this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}