using WCell.Constants.Skills;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Should be a passive spell
    /// Adds the DualWeild skill
    /// </summary>
    public class DualWeildEffectHandler : SpellEffectHandler
    {
        public DualWeildEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if (!(target is Character))
                return;
            SkillCollection skills = ((Character) target).Skills;
            if (skills == null)
                return;
            skills.TryLearn(SkillId.DualWield);
        }
    }
}