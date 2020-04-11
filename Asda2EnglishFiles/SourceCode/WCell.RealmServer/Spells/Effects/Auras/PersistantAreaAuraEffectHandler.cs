using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Creates a Dynamic Object, which -contrary to what its name suggests- is a static animation in the world and
    /// applies a static <see cref="T:WCell.RealmServer.Spells.Auras.AreaAura">AreaAura</see> to everyone who is within the radius of influence
    /// </summary>
    public class PersistantAreaAuraEffectHandler : SpellEffectHandler
    {
        public PersistantAreaAuraEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }
    }
}