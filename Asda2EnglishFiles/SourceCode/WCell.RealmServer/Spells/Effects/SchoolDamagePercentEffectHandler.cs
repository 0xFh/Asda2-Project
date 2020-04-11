using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Deal EffectValue% damage (don't add further modifiers)
    /// </summary>
    public class SchoolDamagePercentEffectHandler : SpellEffectHandler
    {
        public SchoolDamagePercentEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            int dmg = (this.CalcDamageValue() * ((Unit) target).MaxHealth + 50) / 100;
            ((Unit) target).DealSpellDamage(this.m_cast.CasterUnit, this.Effect, dmg, false, true, false, true);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}