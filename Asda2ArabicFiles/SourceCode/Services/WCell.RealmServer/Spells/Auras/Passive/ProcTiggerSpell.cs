/*************************************************************************
 *
 *   file		: ProcTiggerSpell.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-16 21:33:51 +0100 (lø, 16 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1197 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	public class ProcTriggerSpellHandler : AuraEffectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override void OnProc(Unit target, IUnitAction action)
		{
			if (m_spellEffect.TriggerSpell == null)
			{
				log.Warn("Tried to trigger invalid Spell \"{0}\" from Aura {1}", 
					SpellEffect.TriggerSpellId, m_spellEffect.Spell);
			}
			else
			{
				var owner = m_aura.Auras.Owner;
				//owner.SpellCast.Trigger(m_spellEffect.TriggerSpell, m_spellEffect, target);
				owner.SpellCast.Trigger(m_spellEffect.TriggerSpell, target);
			}
		}
	}
};