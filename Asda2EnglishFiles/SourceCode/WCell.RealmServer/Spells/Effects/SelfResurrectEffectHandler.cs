using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Only resurrects oneself and no one else! :)</summary>
    public class SelfResurrectEffectHandler : SpellEffectHandler
    {
        public SelfResurrectEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
            this.Cast.CasterUnit.Health = this.Cast.CasterUnit.MaxHealth * this.CalcEffectValue() / 100;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}