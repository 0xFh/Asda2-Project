using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Learns a new Skill-Tier</summary>
    public class SkillStepEffectHandler : SpellEffectHandler
    {
        public SkillStepEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            SkillId miscValue = (SkillId) this.Effect.MiscValue;
            SkillTierId basePoints = (SkillTierId) this.Effect.BasePoints;
            if (((Character) target).Skills.TryLearn(miscValue, basePoints))
                return;
            this.m_cast.Cancel(SpellFailedReason.MinSkill);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit | ObjectTypes.Player; }
        }
    }
}