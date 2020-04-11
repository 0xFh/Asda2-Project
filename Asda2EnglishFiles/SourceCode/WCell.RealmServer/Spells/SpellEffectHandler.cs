using NLog;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// A SpellEffectHandler handles one SpellEffect during a SpellCast.
    /// Supplied Caster and Target arguments will be checked against the Handler's CasterType and TargetType
    /// properties before the Application of the Effect begins.
    /// The following methods will be called after another, on each of a Spell's SpellEffects:
    /// Before starting:
    /// 1. Init - Initializes all targets (by default adds standard targets) and checks whether this effect can succeed
    /// When performing:
    /// 2. CheckValidTarget - Checks whether this effect may be applied upon the given target
    /// 3. Apply - If none of the effects failed, applies the effect (by default to all targets)
    /// After performing:
    /// 4. Cleanup - Cleans up everything that is not wanted anymore
    /// </summary>
    public abstract class SpellEffectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public readonly SpellEffect Effect;
        protected SpellCast m_cast;
        protected internal SpellTargetCollection m_targets;
        private int CurrentTargetNo;

        protected SpellEffectHandler(SpellCast cast, SpellEffect effect)
        {
            this.m_cast = cast;
            this.Effect = effect;
        }

        public SpellCast Cast
        {
            get { return this.m_cast; }
        }

        public SpellTargetCollection Targets
        {
            get { return this.m_targets; }
        }

        /// <summary>whether Targets need to be initialized</summary>
        public virtual bool HasOwnTargets
        {
            get { return this.Effect.HasTargets; }
        }

        /// <summary>The required Type for the Caster</summary>
        public virtual ObjectTypes CasterType
        {
            get { return ObjectTypes.None; }
        }

        /// <summary>The required Type for all Targets</summary>
        public virtual ObjectTypes TargetType
        {
            get { return ObjectTypes.All; }
        }

        internal SpellFailedReason ValidateAndInitializeTarget(WorldObject target)
        {
            if (!target.CheckObjType(this.TargetType))
                return SpellFailedReason.BadTargets;
            return this.InitializeTarget(target);
        }

        /// <summary>
        /// Initializes this effect and checks whether the effect can be casted *before* Targets have been initialized.
        /// Use CheckValidTarget to validate Targets.
        /// </summary>
        public virtual SpellFailedReason Initialize()
        {
            return SpellFailedReason.Ok;
        }

        /// <summary>
        /// This method is called on every target during CheckApply().
        /// Invalid targets either lead to Spell-Fail or the target being removed from Target-List.
        /// </summary>
        /// <returns>whether the given target is valid.</returns>
        public virtual SpellFailedReason InitializeTarget(WorldObject target)
        {
            return SpellFailedReason.Ok;
        }

        /// <summary>
        /// Apply the effect (by default to all targets of the targettype)
        /// Returns the reason for why it went wrong or SpellFailedReason.None
        /// </summary>
        public virtual void Apply()
        {
            if (this.m_targets != null)
            {
                NPC casterObject = this.Cast.CasterObject as NPC;
                Character casterChar = this.Cast.CasterChar;
                DamageAction[] actions = (DamageAction[]) null;
                for (this.CurrentTargetNo = 0; this.CurrentTargetNo < this.m_targets.Count; ++this.CurrentTargetNo)
                {
                    WorldObject target = this.m_targets[this.CurrentTargetNo];
                    if (target.IsInContext)
                    {
                        this.Apply(target, ref actions);
                        if (this.m_cast == null)
                            return;
                    }
                }

                if (actions == null && this.Effect.Spell.Effect0_EffectType != SpellEffectType.DamageFromPrcAtack &&
                    this.Effect.Spell.Effect0_EffectType != SpellEffectType.InstantKill)
                {
                    if (casterChar != null)
                    {
                        Asda2SpellHandler.SendAnimateSkillStrikeResponse(casterChar, this.Effect.Spell.RealId,
                            (DamageAction[]) null, this.InitialTarget);
                    }
                    else
                    {
                        if (casterObject == null)
                            return;
                        Asda2SpellHandler.SendMonstrUsedSkillResponse(casterObject, this.Effect.Spell.RealId,
                            this.InitialTarget, actions);
                    }
                }
                else
                {
                    if (this.Effect.EffectType == SpellEffectType.CastAnotherSpell || actions == null)
                        return;
                    if (casterChar != null)
                        Asda2SpellHandler.SendAnimateSkillStrikeResponse(casterChar, this.Effect.Spell.RealId, actions,
                            this.InitialTarget);
                    else if (casterObject != null)
                        Asda2SpellHandler.SendMonstrUsedSkillResponse(casterObject, this.Effect.Spell.RealId,
                            this.InitialTarget, actions);
                    else
                        this.Cast.CasterObject.SendMessageToArea(
                            string.Format(
                                "Spell {0} has wrong target[{1}] or caster[{2}]. Please report to developers.",
                                (object) this.Effect.Spell, (object) this.InitialTarget,
                                (object) this.m_cast.CasterUnit), Color.LightGray);
                }
            }
            else
                SpellEffectHandler.log.Warn("SpellEffectHandler has no targets, but Apply() is not overridden: " +
                                            (object) this);
        }

        public Unit InitialTarget { get; set; }

        protected internal void OnChannelTick()
        {
            this.Apply();
        }

        protected internal void OnChannelClose(bool cancelled)
        {
            this.Cleanup();
        }

        /// <summary>Apply the effect to a single target</summary>
        /// <param name="target"></param>
        /// <param name="actions"> </param>
        protected virtual void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }

        /// <summary>Cleans up (if there is anything to clean)</summary>
        protected internal virtual void Cleanup()
        {
            this.m_cast = (SpellCast) null;
            if (this.m_targets == null)
                return;
            this.m_targets.Dispose();
            this.m_targets = (SpellTargetCollection) null;
        }

        /// <summary>
        /// Called automatically after Effect creation, to check for valid caster type:
        /// If invalid, a developer allowed a spell to be casted from the wrong context (or not?)
        /// </summary>
        protected internal void CheckCasterType(ref SpellFailedReason failReason)
        {
            if (this.CasterType == ObjectTypes.None || this.m_cast.CasterObject != null &&
                this.m_cast.CasterObject.CheckObjType(this.CasterType))
                return;
            failReason = SpellFailedReason.Error;
            SpellEffectHandler.log.Warn("Invalid caster {0} for spell {1} in EffectHandler: {2}",
                (object) this.Effect.Spell, (object) this.m_cast.CasterObject, (object) this);
        }

        /// <summary>Used for one-shot damage and healing effects</summary>
        public int CalcDamageValue()
        {
            int val = this.CalcEffectValue();
            if (this.CurrentTargetNo > 0)
                return this.Effect.GetMultipliedValue(this.m_cast.CasterUnit, val, this.CurrentTargetNo);
            return val;
        }

        /// <summary>Used for one-shot damage and healing effects</summary>
        public int CalcDamageValue(int targetNo)
        {
            int val = this.CalcEffectValue();
            if (targetNo > 0)
                return this.Effect.GetMultipliedValue(this.m_cast.CasterUnit, val, targetNo);
            return val;
        }

        public int CalcEffectValue()
        {
            if (this.m_cast.TriggerEffect != null && this.m_cast.TriggerEffect.OverrideEffectValue)
                return this.m_cast.TriggerEffect.CalcEffectValue(this.m_cast.CasterReference);
            return this.Effect.CalcEffectValue(this.m_cast.CasterReference);
        }

        public float GetRadius()
        {
            return this.Effect.GetRadius(this.m_cast.CasterReference);
        }

        public void SendEffectInfoToCaster(string extra)
        {
            if (this.m_cast.CasterChar == null)
                return;
            this.m_cast.CasterChar.SendSystemMessage("SpellEffect {0} {1}", (object) this.GetType(), (object) extra);
        }

        public override string ToString()
        {
            return this.GetType().Name + " - Spell: " + this.Effect.Spell.FullName +
                   (this.m_cast != null ? ", Caster: " + (object) this.m_cast.CasterObject : "");
        }
    }
}