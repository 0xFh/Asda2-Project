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
        if(Cast == null || Cast.CasterUnit == null || target == null)
          return;
        int length = Effect.MiscValueB;
        if(length == 0 && Effect.MiscValue > 0)
          length = 1;
        DamageAction[] values = new DamageAction[length];
        for(int index = 0; index < length; ++index)
        {
          values[index] = ((Unit) target).DealSpellDamage(m_cast.CasterUnit, Effect,
            CalcPrcBoostDamageValue(), true, true, false, false);
          if(Effect.Spell.RealId == 709)
          {
            float hp = MathUtil.ClampMinMax(values[index].ActualDamage * 0.7f, 0.0f,
              Cast.CasterUnit.MaxHealth * 0.3f);
            Cast.CasterUnit.Map.CallDelayed((int) Effect.Spell.CastDelay + 200,
              () => Cast.CasterUnit.Heal((int) hp, null, null));
          }
        }

        if(actions == null)
          actions = values;
        else
          ArrayUtil.Concat(ref actions, values);
      }
      catch(NullReferenceException ex)
      {
      }
    }

    private int CalcPrcBoostDamageValue()
    {
      float num1 = Effect.MiscValue / 100f;
      int num2 = 0;
      int num3;
      if(m_cast.CasterUnit is NPC)
      {
        num3 = (int) m_cast.CasterUnit.MainWeapon.Damages[0].Minimum;
      }
      else
      {
        num2 = m_cast.CasterChar.GetRandomMagicDamage();
        num3 = (int) m_cast.CasterChar.GetRandomPhysicalDamage();
      }

      if(num2 > num3)
        return (int) (num2 * (0.05 + num1));
      return (int) (num3 * (0.05 + num1));
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}