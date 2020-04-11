using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class WMORepair : SpellEffectHandler
    {
        public WMORepair(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            if (!(target is GameObject))
                return SpellFailedReason.NoValidTargets;
            return ((GameObject) target).GOType != GameObjectType.DestructibleBuilding
                ? SpellFailedReason.BadTargets
                : SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.GameObject; }
        }
    }
}