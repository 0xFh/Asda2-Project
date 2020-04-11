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
            Spell triggerSpell = this.Effect.TriggerSpell;
            if (triggerSpell == null)
                return;
            foreach (WorldObject worldObject in this.Cast.Targets.Where<WorldObject>(
                (Func<WorldObject, bool>) (target =>
                {
                    if (target != null)
                        return target.IsInWorld;
                    return false;
                })))
                worldObject.SpellCast.TriggerSelf(triggerSpell);
            base.Apply();
        }
    }
}