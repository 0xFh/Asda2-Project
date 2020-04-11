using System;
using System.Linq;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
  public class TriggerSpellFromTargetWithCasterAsTargetHandler : SpellEffectHandler
  {
    public TriggerSpellFromTargetWithCasterAsTargetHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override void Apply()
    {
      Spell triggerSpell = Effect.TriggerSpell;
      if(triggerSpell == null)
        return;
      foreach(WorldObject worldObject in Cast.Targets.Where(
        target =>
        {
          if(target != null)
            return target.IsInWorld;
          return false;
        }))
        worldObject.SpellCast.TriggerSelf(triggerSpell);
      base.Apply();
    }
  }
}