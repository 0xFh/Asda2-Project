using System;
using System.Linq;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
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

                if (Cast == null || Cast.CasterUnit == null || target == null)
                    return;
                var hits = Effect.MiscValueB;
                if (hits == 0 && Effect.MiscValue > 0)
                    hits = 1;

                var acs = new DamageAction[hits];
                for (int i = 0; i < hits; i++)
                {
                    acs[i] = ((Unit)target).DealSpellDamage(m_cast.CasterUnit, Effect, CalcPrcBoostDamageValue(), true, true, false, false);
                    if (Effect.Spell.RealId == 709)//StealLife
                    {
                        var hp = MathUtil.ClampMinMax(acs[i].ActualDamage * 0.7f, 0, Cast.CasterUnit.MaxHealth * 0.3f);
                        Cast.CasterUnit.Map.CallDelayed((int)(Effect.Spell.CastDelay + 200), () => Cast.CasterUnit.Heal((int)hp));
                    }

                }
                if (actions == null)
                    actions = acs;
                else
                {
                    ArrayUtil.Concat(ref actions, acs);
                }
            }
            catch (NullReferenceException)
            {
            }
        }


        private int CalcPrcBoostDamageValue()
        {
            var prcBoost = ((float)Effect.MiscValue) / 100;

            int mDmg = 0;
            int pDmg;
            if (m_cast.CasterUnit is NPC)
            {
                pDmg = (int)m_cast.CasterUnit.MainWeapon.Damages[0].Minimum;
            }
            else
            {
                mDmg = (m_cast.CasterChar.GetRandomMagicDamage());
                pDmg = (int)(m_cast.CasterChar.GetRandomPhysicalDamage());
            }
            if (mDmg > pDmg)
                return (int)(mDmg * (0.05 + prcBoost));
            return (int)(pDmg * (0.05 + prcBoost));
        }

        public override ObjectTypes TargetType
        {
            get
            {
                return ObjectTypes.Unit;
            }
        }
    }
}