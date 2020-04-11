using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class EnergizePctEffectHandler : EnergizeEffectHandler
    {
        public EnergizePctEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if ((PowerType) this.Effect.MiscValue != ((Unit) target).PowerType)
                return;
            int num = (this.m_cast.CasterUnit.MaxPower * this.CalcEffectValue() + 50) / 100;
            ((Unit) target).Energize(num, this.m_cast.CasterUnit, this.Effect);
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Object; }
        }
    }
}