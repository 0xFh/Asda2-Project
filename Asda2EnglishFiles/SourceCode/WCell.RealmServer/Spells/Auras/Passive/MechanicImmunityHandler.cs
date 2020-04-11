using System;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class MechanicImmunityHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (this.SpellEffect.MiscValue == -1)
            {
                this.m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Stunned, this.m_aura.Spell);
                this.m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Asleep, this.m_aura.Spell);
                this.m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Fleeing, this.m_aura.Spell);
                this.m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Frozen, this.m_aura.Spell);
                this.m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Horrified, this.m_aura.Spell);
                this.Owner.Auras.RemoveWhere((Predicate<Aura>) (aura =>
                {
                    if (aura.Spell.Mechanic != SpellMechanic.Stunned && aura.Spell.Mechanic != SpellMechanic.Asleep &&
                        (aura.Spell.Mechanic != SpellMechanic.Fleeing && aura.Spell.Mechanic != SpellMechanic.Frozen))
                        return aura.Spell.Mechanic == SpellMechanic.Horrified;
                    return true;
                }));
            }
            else
            {
                this.m_aura.Auras.Owner.IncMechImmunityCount((SpellMechanic) this.m_spellEffect.MiscValue,
                    this.m_aura.Spell);
                this.Owner.Auras.RemoveWhere((Predicate<Aura>) (aura =>
                    aura.Spell.Mechanic == (SpellMechanic) this.m_spellEffect.MiscValue));
            }
        }

        protected override void Remove(bool cancelled)
        {
            if (this.SpellEffect.MiscValue == -1)
            {
                this.m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Stunned);
                this.m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Asleep);
                this.m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Fleeing);
                this.m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Frozen);
                this.m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Horrified);
            }
            else
                this.m_aura.Auras.Owner.DecMechImmunityCount((SpellMechanic) this.m_spellEffect.MiscValue);
        }
    }
}