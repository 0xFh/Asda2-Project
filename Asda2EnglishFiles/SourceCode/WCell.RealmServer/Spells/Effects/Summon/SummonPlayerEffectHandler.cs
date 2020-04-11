using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class SummonPlayerEffectHandler : SummonEffectHandler
    {
        public SummonPlayerEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            Character character = (Character) target;
            if (character.SummonRequest != null)
                return SpellFailedReason.AlreadyHaveSummon;
            return !character.MayTeleport ? SpellFailedReason.BadTargets : SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            ((Character) target).StartSummon((ISummoner) this.m_cast.CasterUnit);
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Player; }
        }
    }
}