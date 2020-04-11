using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>TODO: Target gets a res query</summary>
    public class ResurrectFlatEffectHandler : SpellEffectHandler
    {
        public ResurrectFlatEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            Unit unit;
            if (target is Unit)
            {
                unit = (Unit) target;
            }
            else
            {
                if (!(target is Corpse))
                    return;
                unit = (Unit) ((Corpse) target).Owner;
                if (unit == null || unit.IsAlive)
                    return;
            }

            unit.Health = this.CalcEffectValue();
            if (((Unit) target).PowerType != PowerType.Mana)
                return;
            ((Unit) target).Energize(this.Effect.MiscValue, this.m_cast.CasterUnit, this.Effect);
        }
    }
}