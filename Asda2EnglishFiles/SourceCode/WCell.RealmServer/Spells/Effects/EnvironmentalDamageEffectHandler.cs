using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Mostly caused by burning fires</summary>
    public class EnvironmentalDamageEffectHandler : SpellEffectHandler
    {
        public EnvironmentalDamageEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject targetObj, ref DamageAction[] actions)
        {
            int dmg = this.CalcEffectValue();
            Unit unit = (Unit) targetObj;
            if (dmg < 100)
                dmg = Math.Min(1, dmg * unit.MaxHealth / 100);
            unit.DealSpellDamage(this.m_cast.CasterUnit, this.Effect, dmg, true, true, false, true);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}