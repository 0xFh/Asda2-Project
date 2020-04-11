using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class PowerDrainEffectHandler : SpellEffectHandler
    {
        public PowerDrainEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            return ((Unit) target).MaxPower == 0 || ((Unit) target).PowerType != (PowerType) this.Effect.MiscValue
                ? SpellFailedReason.BadTargets
                : SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            PowerType miscValue = (PowerType) this.Effect.MiscValue;
            int amount = this.CalcEffectValue();
            if (miscValue == PowerType.Happiness)
                amount /= 1000;
            ((Unit) target).LeechPower(amount, this.Effect.RealPointsPerLevel, this.m_cast.CasterUnit, this.Effect);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}