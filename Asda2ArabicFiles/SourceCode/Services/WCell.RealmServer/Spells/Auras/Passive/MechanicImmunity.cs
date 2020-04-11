/*************************************************************************
 *
 *   file		: MechanicImmunity.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

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
                Owner.Auras.RemoveWhere(aura => aura.Spell.Mechanic == SpellMechanic.Stunned || aura.Spell.Mechanic == SpellMechanic.Asleep || aura.Spell.Mechanic == SpellMechanic.Fleeing || aura.Spell.Mechanic == SpellMechanic.Frozen || aura.Spell.Mechanic == SpellMechanic.Horrified);
            }
            else
            {
                m_aura.Auras.Owner.IncMechImmunityCount((SpellMechanic)m_spellEffect.MiscValue, m_aura.Spell);
                Owner.Auras.RemoveWhere(aura => aura.Spell.Mechanic == (SpellMechanic)m_spellEffect.MiscValue);
            }
		}

		protected override void Remove(bool cancelled)
		{
            if (SpellEffect.MiscValue == -1)
            {
                m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Stunned);
                m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Asleep);
                m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Fleeing);
                m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Frozen);
                m_aura.Auras.Owner.DecMechImmunityCount(SpellMechanic.Horrified);
            }
            else
            {

                m_aura.Auras.Owner.DecMechImmunityCount((SpellMechanic)m_spellEffect.MiscValue);
            }
		}

	}
};