using System;
using WCell.Constants;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Adds damage absorption.
  /// 
  /// There are two kinds of absorbtions:
  /// 1. 100% absorbtion, up until a max is absorbed (usually)
  /// 2. Less than 100% absorption until time runs out (or max is absorbed -&gt; Needs customization, since its usually different each time)
  /// </summary>
  public class SchoolAbsorbHandler : AttackEventEffectHandler
  {
    public int RemainingValue;

    protected override void Apply()
    {
      if(SpellEffect.MiscValueC == 0)
        RemainingValue = SpellEffect.MiscValue;
      else if(SpellEffect.MiscValueB == 1)
        RemainingValue = (int) (SpellEffect.MiscValue *
                                (double) m_aura.CasterUnit.RandomMagicDamage / 100.0);
      base.Apply();
    }

    public override void OnDefend(DamageAction action)
    {
      RemainingValue = action.Absorb(RemainingValue, (DamageSchoolMask) m_spellEffect.MiscValueC);
      if(RemainingValue > 0)
        return;
      Owner.AddMessage(m_aura.Cancel);
    }
  }
}