using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects.Custom
{
    /// <summary>
    /// Removes the cooldown for the SpellEffect.AffectSpellSet
    /// </summary>
    public class RemoveCooldownEffectHandler : SpellEffectHandler
    {
        public RemoveCooldownEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (this.Effect.AffectSpellSet != null)
                return SpellFailedReason.Ok;
            LogManager.GetCurrentClassLogger()
                .Warn("Tried to use {0} in Spell \"{1}\" with an empty SpellEffect.AffectSpellSet",
                    (object) this.GetType(), (object) this.Effect.Spell);
            return SpellFailedReason.Error;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if (!(target is Unit))
                return;
            foreach (Spell affectSpell in this.Effect.AffectSpellSet)
                ((Unit) target).Spells.ClearCooldown(affectSpell, false);
        }
    }
}