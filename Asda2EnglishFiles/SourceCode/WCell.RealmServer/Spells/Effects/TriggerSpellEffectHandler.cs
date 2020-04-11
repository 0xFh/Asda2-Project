using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Triggers a spell on this Effect's targets</summary>
    public class TriggerSpellEffectHandler : SpellEffectHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public TriggerSpellEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
            if (this.Effect.TriggerSpell == null)
                TriggerSpellEffectHandler.log.Warn("Tried to cast Spell \"{0}\" which has invalid TriggerSpellId {1}",
                    (object) this.Effect.Spell, (object) this.Effect.TriggerSpellId);
            else
                this.TriggerSpell(this.Effect.TriggerSpell);
        }

        protected void TriggerSpell(Spell triggerSpell)
        {
            this.m_cast.Trigger(triggerSpell, this.Effect,
                triggerSpell.Effects.Length != 1 || this.m_targets == null
                    ? (WorldObject[]) null
                    : this.m_targets.ToArray());
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }
    }
}