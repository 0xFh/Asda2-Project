using System;
using System.Linq;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{
    internal class RemoveImpairingEffectsHandler : SpellEffectHandler
    {
        public RemoveImpairingEffectsHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            Character character = target as Character;
            if (character == null)
                return;
            character.Auras.RemoveWhere((Predicate<Aura>) (aura =>
            {
                if (!SpellConstants.MoveMechanics[(int) aura.Spell.Mechanic])
                    return aura.Handlers.Any<AuraEffectHandler>((Func<AuraEffectHandler, bool>) (handler =>
                        SpellConstants.MoveMechanics[(int) handler.SpellEffect.Mechanic]));
                return true;
            }));
        }
    }
}