using System;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class MechanicImmunityHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(SpellEffect.MiscValue == -1)
      {
        m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Stunned, m_aura.Spell);
        m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Asleep, m_aura.Spell);
        m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Fleeing, m_aura.Spell);
        m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Frozen, m_aura.Spell);
        m_aura.Auras.Owner.IncMechImmunityCount(SpellMechanic.Horrified, m_aura.Spell);
        Owner.Auras.RemoveWhere(aura =>
        {
          if(aura.Spell.Mechanic != SpellMechanic.Stunned && aura.Spell.Mechanic != SpellMechanic.Asleep &&
             (aura.Spell.Mechanic != SpellMechanic.Fleeing && aura.Spell.Mechanic != SpellMechanic.Frozen))
            return aura.Spell.Mechanic == SpellMechanic.Horrified;
          return true;
        });
      }
      else
      {
        m_aura.Auras.Owner.IncMechImmunityCount((SpellMechanic) m_spellEffect.MiscValue,
          m_aura.Spell);
        Owner.Auras.RemoveWhere(aura =>
          aura.Spell.Mechanic == (SpellMechanic) m_spellEffect.MiscValue);
      }
    }

    protected override void Remove(bool cancelled)
    {
      if(SpellEffect.MiscValue == -1)
      {
        m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Stunned);
        m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Asleep);
        m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Fleeing);
        m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Frozen);
        m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Horrified);
      }
      else
        m_aura.Auras.Owner.DecMechImmunityCount((SpellMechanic) m_spellEffect.MiscValue);
    }
  }
}