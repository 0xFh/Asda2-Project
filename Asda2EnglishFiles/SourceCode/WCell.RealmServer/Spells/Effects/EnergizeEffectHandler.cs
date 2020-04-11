using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Adds any kind of Power</summary>
    public class EnergizeEffectHandler : SpellEffectHandler
    {
        public EnergizeEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            if (((Unit) target).Power != ((Unit) target).MaxPower)
                return base.InitializeTarget(target);
            return ((Unit) target).PowerType != PowerType.Mana
                ? SpellFailedReason.AlreadyAtFullPower
                : SpellFailedReason.AlreadyAtFullMana;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if ((PowerType) this.Effect.MiscValue != ((Unit) target).PowerType)
                return;
            ((Unit) target).Energize(this.CalcEffectValue(), this.m_cast.CasterUnit, this.Effect);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}