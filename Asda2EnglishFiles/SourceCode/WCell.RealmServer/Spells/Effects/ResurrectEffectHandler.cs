using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
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
            Character character = target as Character;
            if (character == null || !character.IsDead)
                return;
            character.Resurrect();
            character.GainXp(character.LastExpLooseAmount * this.Effect.MiscValue, "resurect_spell", false);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}