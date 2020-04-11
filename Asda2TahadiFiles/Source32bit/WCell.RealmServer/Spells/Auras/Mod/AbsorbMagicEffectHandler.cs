using System;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class AbsorbMagicEffectHandler : AttackEventEffectHandler
  {
    public override void OnDefend(DamageAction action)
    {
      if(action.Spell == null || action.Schools.HasFlag(DamageSchoolMask.Physical))
        return;
      action.Victim.Heal(action.ActualDamage, null, null);
      action.Resisted = 100;
      base.OnDefend(action);
    }
  }
}