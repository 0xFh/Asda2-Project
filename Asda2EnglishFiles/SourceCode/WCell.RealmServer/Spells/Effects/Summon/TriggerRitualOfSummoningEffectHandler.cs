using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class TriggerRitualOfSummoningEffectHandler : SpellEffectHandler
    {
        public TriggerRitualOfSummoningEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            return this.m_cast.InitialTargets == null || this.m_cast.InitialTargets.Length == 0 ||
                   !(this.m_cast.InitialTargets[0] is Unit)
                ? SpellFailedReason.NoValidTargets
                : SpellFailedReason.Ok;
        }

        public override void Apply()
        {
            this.m_cast.Trigger(this.Effect.TriggerSpell, this.m_cast.InitialTargets);
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Player; }
        }
    }
}