using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class WeaponDamageNoSchoolEffectHandler : SpellEffectHandler
    {
        public WeaponDamageNoSchoolEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}