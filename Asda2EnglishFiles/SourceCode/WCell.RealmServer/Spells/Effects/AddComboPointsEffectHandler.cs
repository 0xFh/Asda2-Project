using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class AddComboPointsEffectHandler : SpellEffectHandler
    {
        public AddComboPointsEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        /// <summary>Applying combopoints cannot result in a fail</summary>
        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            this.m_cast.CasterUnit.ModComboState((Unit) target, this.CalcEffectValue());
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}