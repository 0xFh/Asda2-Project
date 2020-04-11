using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Applies a single AuraEffectHandler to every target</summary>
    public class ApplyAuraEffectHandler : SpellEffectHandler
    {
        private List<SingleAuraApplicationInfo> m_auraEffectHandlers;

        public ApplyAuraEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        /// <summary>Aura-Spells should always have targets</summary>
        public override bool HasOwnTargets
        {
            get { return true; }
        }

        public override SpellFailedReason Initialize()
        {
            this.m_auraEffectHandlers = new List<SingleAuraApplicationInfo>(3);
            return SpellFailedReason.Ok;
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            SpellFailedReason failedReason = SpellFailedReason.Ok;
            AuraEffectHandler auraEffectHandler = this.Effect.CreateAuraEffectHandler(this.m_cast.CasterReference,
                (Unit) target, ref failedReason, this.m_cast);
            if (failedReason == SpellFailedReason.Ok)
                this.m_auraEffectHandlers.Add(new SingleAuraApplicationInfo((Unit) target, auraEffectHandler));
            return failedReason;
        }

        public override void Apply()
        {
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }

        /// <summary>
        /// Adds all AuraEffectHandlers that this SpellEffectHandler created,
        /// indexed by the target they have been created for (if this effect creates Auras at all)
        /// </summary>
        public void AddAuraHandlers(List<AuraApplicationInfo> applicationInfos)
        {
            foreach (SingleAuraApplicationInfo auraEffectHandler in this.m_auraEffectHandlers)
            {
                foreach (AuraApplicationInfo applicationInfo in applicationInfos)
                {
                    if (applicationInfo.Target == auraEffectHandler.Target && applicationInfo.Handlers != null)
                    {
                        applicationInfo.Handlers.Add(auraEffectHandler.Handler);
                        break;
                    }
                }
            }
        }
    }
}