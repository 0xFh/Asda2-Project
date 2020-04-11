using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{
    public class DispelMechanicEffectHandler : SpellEffectHandler
    {
        public DispelMechanicEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            ((Unit) target).Auras.RemoveWhere((Predicate<Aura>) (aura => aura.Spell.Mechanic != SpellMechanic.None));
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}