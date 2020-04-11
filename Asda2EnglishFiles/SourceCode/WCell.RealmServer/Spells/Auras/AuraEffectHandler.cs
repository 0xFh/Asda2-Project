using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>An AuraEffectHandler handles the behavior of an aura</summary>
    public abstract class AuraEffectHandler
    {
        protected internal Aura m_aura;
        protected internal SpellEffect m_spellEffect;
        public int BaseEffectValue;
        private bool m_IsActivated;

        /// <summary>
        /// The value of the underlying SpellEffect that was calculated when
        /// this Aura was last applied or refreshed (see <see cref="M:WCell.RealmServer.Spells.SpellEffect.CalcEffectValue(WCell.RealmServer.Entities.Unit)" />).
        /// The value is multiplied by the StackCount of the Aura (<see cref="P:WCell.RealmServer.Spells.Auras.Aura.StackCount" />).
        /// </summary>
        public int EffectValue
        {
            get
            {
                if (this.m_aura != null && this.m_aura.Spell.CanStack && this.m_aura.StackCount > 1)
                    return this.BaseEffectValue * this.m_aura.StackCount;
                return this.BaseEffectValue;
            }
        }

        public Unit Holder
        {
            get { return this.m_aura.Auras.Owner; }
        }

        protected internal void Init(Aura aura)
        {
            this.m_aura = aura;
        }

        /// <summary>
        /// whether this is a positive effect (by default: If they have a positive value)
        /// </summary>
        public virtual bool IsPositive
        {
            get { return this.EffectValue >= 0; }
        }

        public bool IsActivated
        {
            get { return this.m_IsActivated; }
            internal set
            {
                if (this.m_IsActivated == value)
                    return;
                if (this.m_IsActivated = value)
                    this.Apply();
                else
                    this.Remove(false);
            }
        }

        /// <summary>The Aura to which this AuraEffect belongs</summary>
        public Aura Aura
        {
            get { return this.m_aura; }
        }

        public Unit Owner
        {
            get { return this.m_aura.Auras.Owner; }
        }

        /// <summary>
        /// The SpellEffect which created this AuraEffect OR:
        /// If the Aura was triggered by another Spell and the original SpellEffect had OverrideEffectValue = true,
        /// this is the SpellEffect that triggered the creation of the Aura (through TriggerSpell, ProcTriggerSpell etc).
        /// </summary>
        public SpellEffect SpellEffect
        {
            get { return this.m_spellEffect; }
        }

        public void UpdateEffectValue()
        {
            this.BaseEffectValue = this.m_spellEffect.CalcEffectValue(this.m_aura.CasterUnit);
        }

        /// <summary>
        /// Check whether this handler can be applied to the given target.
        /// m_aura, as well as some other fields are not set when this method gets called.
        /// </summary>
        protected internal virtual void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
            Unit target, ref SpellFailedReason failReason)
        {
        }

        /// <summary>To be called by Aura.Apply on periodic effects</summary>
        internal void DoApply()
        {
            if (this.m_IsActivated && !this.m_spellEffect.IsPeriodic)
                return;
            this.m_IsActivated = true;
            this.Apply();
        }

        /// <summary>To be called by Aura.Apply on periodic effects</summary>
        internal void DoRemove(bool cancelled)
        {
            if (!this.m_IsActivated)
                return;
            this.m_IsActivated = false;
            this.Remove(cancelled);
        }

        /// <summary>Applies this EffectHandler's effect to its holder</summary>
        protected virtual void Apply()
        {
        }

        /// <summary>Removes the effect from its holder</summary>
        protected virtual void Remove(bool cancelled)
        {
        }

        /// <summary>
        /// Whether this proc handler can be triggered by the given action
        /// </summary>
        public virtual bool CanProcBeTriggeredBy(IUnitAction action)
        {
            return true;
        }

        /// <summary>
        /// Called when a matching proc event triggers this proc handler with the given
        /// triggerer and action.
        /// </summary>
        public virtual void OnProc(Unit triggerer, IUnitAction action)
        {
        }
    }
}