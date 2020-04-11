using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ManaShieldHandler : AttackEventEffectHandler
  {
    private float factor;
    private float factorInverse;
    private int remaining;

    protected override void Apply()
    {
      factor = (double) SpellEffect.ProcValue != 0.0 ? SpellEffect.ProcValue : 1f;
      factorInverse = 1f / factor;
      remaining = EffectValue;
      base.Apply();
    }

    public override void OnDefend(DamageAction action)
    {
      Unit owner = Owner;
      int power = owner.Power;
      int damage = action.Damage;
      int num1 = Math.Min(damage, (int) (power * (double) factorInverse));
      if(remaining < num1)
      {
        num1 = remaining;
        remaining = 0;
        m_aura.Remove(false);
      }
      else
        remaining -= num1;

      int num2 = (int) (num1 * (double) factor);
      Unit casterUnit = Aura.CasterUnit;
      if(casterUnit != null)
        num2 = casterUnit.Auras.GetModifiedInt(SpellModifierType.HealingOrPowerGain, m_spellEffect.Spell,
          num2);
      owner.Power = power - num2;
      int num3 = damage - num2;
      action.Damage = num3;
    }
  }
}