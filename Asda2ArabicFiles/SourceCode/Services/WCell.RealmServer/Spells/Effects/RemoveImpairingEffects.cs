﻿using System.Linq;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    class RemoveImpairingEffectsHandler : SpellEffectHandler
	{
		public RemoveImpairingEffectsHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{}
        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            var chr = target as Character;
            if (chr != null)
            {
                chr.Auras.RemoveWhere(aura => SpellConstants.MoveMechanics[(int)aura.Spell.Mechanic] || aura.Handlers.Any(handler => SpellConstants.MoveMechanics[(int)handler.SpellEffect.Mechanic]));
            }
        }
	}
}
