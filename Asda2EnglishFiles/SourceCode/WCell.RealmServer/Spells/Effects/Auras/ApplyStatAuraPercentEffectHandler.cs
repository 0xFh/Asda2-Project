using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Weird stuff:
    /// Mostly like ApplyAura but often (not always!) applies to all enemies around the caster, although target = Self.
    /// </summary>
    public class ApplyStatAuraPercentEffectHandler : ApplyAuraEffectHandler
    {
        public ApplyStatAuraPercentEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }
    }
}