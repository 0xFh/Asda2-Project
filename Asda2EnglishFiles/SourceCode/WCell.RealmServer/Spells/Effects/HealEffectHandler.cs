using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Either just heals or trades one Rejuvenation or Regrowth for a lot of healing
    /// </summary>
    public class HealEffectHandler : SpellEffectHandler
    {
        public HealEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            ((Unit) target).Heal(
                this.m_cast.CasterUnit.AddHealingModsToAction(
                    (int) (((double) this.m_cast.CasterUnit.RandomMagicDamage / 500.0 + 1.0) *
                           (double) this.Effect.MiscValue), this.Effect, DamageSchool.Magical), this.m_cast.CasterUnit,
                this.Effect);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}