using NLog;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    public class ForgetSpecializationEffectHandler : SpellEffectHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public ForgetSpecializationEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            Spell triggerSpell = this.Effect.TriggerSpell;
            if (triggerSpell != null)
                ((Unit) target).Spells.Remove(triggerSpell);
            else
                ForgetSpecializationEffectHandler.log.Warn("Couldn't find skill to forget: " +
                                                           (object) this.Effect.Spell);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Player; }
        }
    }
}