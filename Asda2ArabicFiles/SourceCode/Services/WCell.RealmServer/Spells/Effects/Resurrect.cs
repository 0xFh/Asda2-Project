/*************************************************************************
 *
 *   file		: Resurrect.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $

 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Updates;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
	public class ResurrectEffectHandler : SpellEffectHandler
	{
		public ResurrectEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target, ref DamageAction[] actions)
		{
		    var chr = target as Character;
		    if (chr != null && chr.IsDead)
		    {
                chr.Resurrect();
                chr.GainXp(chr.LastExpLooseAmount*Effect.MiscValue,"resurect_spell");
                Asda2TitleChecker.OnResurectUse(Cast.CasterChar);
		    }
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Unit;
			}
		}
	}
}