using WCell.Constants.Updates;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class WeaponDamageEffectHandler : SpellEffectHandler
    {
        public WeaponDamageEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
        }

        public virtual void OnHit(DamageAction action)
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