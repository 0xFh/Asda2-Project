using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
    internal class DamageFromPrcAtackHandler : SpellEffectHandler
    {
        public DamageFromPrcAtackHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            try
            {
                if (this.Cast == null || this.Cast.CasterUnit == null || target == null)
                    return;
                int length = this.Effect.MiscValueB;
                if (length == 0 && this.Effect.MiscValue > 0)
                    length = 1;
                DamageAction[] values = new DamageAction[length];
                for (int index = 0; index < length; ++index)
                {
                    values[index] = ((Unit) target).DealSpellDamage(this.m_cast.CasterUnit, this.Effect,
                        this.CalcPrcBoostDamageValue(), true, true, false, false);
                    if (this.Effect.Spell.RealId == (short) 709)
                    {
                        float hp = MathUtil.ClampMinMax((float) values[index].ActualDamage * 0.7f, 0.0f,
                            (float) this.Cast.CasterUnit.MaxHealth * 0.3f);
                        this.Cast.CasterUnit.Map.CallDelayed((int) this.Effect.Spell.CastDelay + 200,
                            (Action) (() => this.Cast.CasterUnit.Heal((int) hp, (Unit) null, (SpellEffect) null)));
                    }
                }

                if (actions == null)
                    actions = values;
                else
                    ArrayUtil.Concat<DamageAction>(ref actions, values);
            }
            catch (NullReferenceException ex)
            {
            }
        }

        private int CalcPrcBoostDamageValue()
        {
            float num1 = (float) this.Effect.MiscValue / 100f;
            int num2 = 0;
            int num3;
            if (this.m_cast.CasterUnit is NPC)
            {
                num3 = (int) this.m_cast.CasterUnit.MainWeapon.Damages[0].Minimum;
            }
            else
            {
                num2 = this.m_cast.CasterChar.GetRandomMagicDamage();
                num3 = (int) this.m_cast.CasterChar.GetRandomPhysicalDamage();
            }

            if (num2 > num3)
                return (int) ((double) num2 * (0.05 + (double) num1));
            return (int) ((double) num3 * (0.05 + (double) num1));
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}