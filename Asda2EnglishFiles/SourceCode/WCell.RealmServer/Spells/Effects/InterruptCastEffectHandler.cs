using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class InterruptCastEffectHandler : SpellEffectHandler
    {
        public InterruptCastEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            return base.InitializeTarget(target);
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if (!target.IsUsingSpell)
                return;
            target.SpellCast.Cancel(SpellFailedReason.Interrupted);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}