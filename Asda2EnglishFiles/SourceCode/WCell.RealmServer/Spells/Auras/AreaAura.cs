using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>
    /// An AreaAura either (if its using an AreaAura spell) applies AreaAura-Effects to everyone in a radius around its center or
    /// triggers its spell on everyone around it.
    /// Can be persistant (Blizzard, Hurricane etc.) or mobile (Paladin Aura)
    /// </summary>
    public class AreaAura : IUpdatable, IAura
    {
        /// <summary>
        /// Delay in milliseconds to wait before revalidating effected targets (Default: 1000 ms).
        /// This is mostly used for Paladin Auras because they don't have an amplitude on their own.
        /// </summary>
        [Variable("DefaultAreaAuraAmplitude")] public static int DefaultAmplitude = 1000;

        private WorldObject m_holder;
        private Spell m_spell;
        private Dictionary<Unit, Aura> m_targets;
        private float m_radius;
        private ITickTimer m_controller;
        private TimerEntry m_timer;
        private ObjectReference m_CasterReference;
        private int m_duration;
        private int m_elapsed;
        private ISpellParameters m_params;
        private int m_remainingCharges;
        private bool m_IsActivated;

        /// <summary>
        /// Creates a new AreaAura that will auto-trigger the given Spell on everyone, according
        /// to the given SpellParameters.
        /// </summary>
        /// <param name="distributedSpell"></param>
        public AreaAura(WorldObject holder, Spell distributedSpell, ISpellParameters prms)
        {
            this.Init(holder, distributedSpell);
            this.m_params = prms;
            this.m_remainingCharges = this.m_params.MaxCharges;
            this.m_radius = (float) prms.Radius;
            this.Start((ITickTimer) null, true);
        }

        public AreaAura(WorldObject holder, Spell spell)
        {
            this.Init(holder, spell);
            this.m_radius = spell.Effects[0].GetRadius(holder.SharedReference);
        }

        /// <summary>
        /// Creates a new AreaAura which applies its effects to everyone in its radius of influence
        /// </summary>
        protected void Init(WorldObject holder, Spell spell)
        {
            this.m_holder = holder;
            this.m_CasterReference =
                !(holder is DynamicObject) ? holder.SharedReference : holder.Master.SharedReference;
            this.m_spell = spell;
            if (spell.IsAreaAura)
                this.m_targets = new Dictionary<Unit, Aura>();
            holder.AddAreaAura(this);
        }

        /// <summary>The Holder of this AreaAura.</summary>
        public WorldObject Holder
        {
            get { return this.m_holder; }
            internal set { this.m_holder = value; }
        }

        public Spell Spell
        {
            get { return this.m_spell; }
        }

        /// <summary>
        /// The Position of the holder is also the Center of the Aura.
        /// </summary>
        public Vector3 Center
        {
            get { return this.m_holder.Position; }
        }

        /// <summary>Radius of the Aura</summary>
        public float Radius
        {
            get { return this.m_radius; }
            set { this.m_radius = value; }
        }

        /// <summary>Milliseconds until this expires</summary>
        public int TimeLeft
        {
            get
            {
                if (this.m_controller == null)
                    return this.m_duration - this.m_elapsed;
                return this.m_controller.TimeLeft;
            }
        }

        /// <summary>
        /// Aura is active if its still applied to a <c>Holder</c>
        /// </summary>
        public bool IsAdded
        {
            get { return this.m_holder != null; }
        }

        /// <summary>
        /// Whether this AreaAura is currently activated and applies it's effects to the area
        /// </summary>
        public bool IsActivated
        {
            get { return this.m_IsActivated; }
            set
            {
                if (this.m_IsActivated == value)
                    return;
                this.m_IsActivated = value;
                if (value)
                {
                    if (this.m_timer == null)
                        return;
                    this.m_timer.Start();
                }
                else
                {
                    if (this.m_timer != null)
                        this.m_timer.Stop();
                    if (this.m_targets == null)
                        return;
                    AreaAura.RemoveEffects((IEnumerable<KeyValuePair<Unit, Aura>>) this.m_targets);
                    this.m_targets.Clear();
                }
            }
        }

        /// <summary>Called by a SpellChannel when channeling</summary>
        public void Apply()
        {
            this.RevalidateTargetsAndApply(0);
        }

        /// <summary>
        /// Initializes this AreaAura with the given controller.
        /// If no controller is given, the AreaAura controls timing and disposal itself.
        /// </summary>
        /// <param name="controller">A controller controls timing and disposal of this AreaAura</param>
        /// <param name="noTimeout">whether the Aura should not expire (ignore the Spell's duration).</param>
        public void Start(ITickTimer controller, bool noTimeout)
        {
            if (this.m_IsActivated)
                return;
            if ((double) this.m_radius == 0.0)
                this.m_radius = 5f;
            this.m_controller = controller;
            if (this.m_controller == null || this.m_controller.MaxTicks == 1)
                this.m_timer = this.m_params == null
                    ? new TimerEntry(AreaAura.DefaultAmplitude, AreaAura.DefaultAmplitude,
                        new Action<int>(this.RevalidateTargetsAndApply))
                    : new TimerEntry(this.m_params.StartDelay,
                        this.m_params.Amplitude != 0 ? this.m_params.Amplitude : AreaAura.DefaultAmplitude,
                        new Action<int>(this.RevalidateTargetsAndApply));
            if (noTimeout)
            {
                this.m_duration = int.MaxValue;
            }
            else
            {
                this.m_duration = this.m_spell.GetDuration(this.m_CasterReference);
                if (this.m_duration < 1)
                    this.m_duration = int.MaxValue;
            }

            this.IsActivated = true;
        }

        public void TryRemove(bool cancelled)
        {
        }

        /// <summary>Remove and dispose AreaAura.</summary>
        public void Remove(bool cancelled)
        {
            this.IsActivated = false;
            if (this.m_holder != null)
                this.m_holder.CancelAreaAura(this);
            this.m_holder = (WorldObject) null;
            this.m_remainingCharges = 0;
            if (this.m_timer == null)
                return;
            this.m_timer.Dispose();
        }

        /// <summary>
        /// Check for all targets in radius, kick out invalid ones and add new ones
        /// </summary>
        protected internal void RevalidateTargetsAndApply(int timeElapsed)
        {
            if (this.m_controller == null)
            {
                this.m_elapsed += timeElapsed;
                if (this.m_elapsed >= this.m_duration)
                {
                    this.Remove(false);
                    return;
                }
            }

            this.RemoveInvalidTargets();
            bool auraEffects = this.m_spell.AreaAuraEffects != null;
            List<WorldObject> newTargets = new List<WorldObject>();
            bool exclMobs = this.m_holder.Faction.Id == FactionId.None;
            this.m_holder.IterateEnvironment(this.m_radius, (Func<WorldObject, bool>) (obj =>
            {
                if (obj != this.m_holder && (exclMobs && obj.IsPlayerOwned || !exclMobs && obj is Unit) &&
                    (this.m_spell.HasHarmfulEffects == this.m_holder.MayAttack((IFactionMember) obj) &&
                     this.m_spell.CheckValidTarget(this.m_holder, obj) == SpellFailedReason.Ok &&
                     (!auraEffects || !this.m_targets.ContainsKey((Unit) obj))))
                    newTargets.Add(obj);
                return true;
            }));
            for (int index = 0; index < newTargets.Count; ++index)
            {
                Unit unit = (Unit) newTargets[index];
                if (!this.IsAdded)
                    break;
                if (auraEffects)
                    this.ApplyAuraEffects(unit);
                else
                    this.m_holder.SpellCast.Trigger(this.m_spell, new WorldObject[1]
                    {
                        (WorldObject) unit
                    });
                if (this.m_holder.IsTrap)
                    this.OnTrapTriggered(unit);
                if (this.m_remainingCharges != 0)
                {
                    --this.m_remainingCharges;
                    if (this.m_remainingCharges == 0)
                        this.Remove(false);
                }
            }
        }

        /// <summary>
        /// Called when the holder is a trap and the given triggerer triggered it.
        /// </summary>
        /// <param name="triggerer"></param>
        private void OnTrapTriggered(Unit triggerer)
        {
            Unit owner = ((GameObject) this.m_holder).Owner;
            if (owner == null)
                return;
            Unit unit = triggerer;
            int num1 = 2097152;
            Unit triggerer1 = triggerer;
            TrapTriggerAction trapTriggerAction1 = new TrapTriggerAction();
            trapTriggerAction1.Attacker = owner;
            trapTriggerAction1.Spell = this.m_spell;
            trapTriggerAction1.Victim = triggerer;
            TrapTriggerAction trapTriggerAction2 = trapTriggerAction1;
            int num2 = 0;
            int num3 = 0;
            unit.Proc((ProcTriggerFlags) num1, triggerer1, (IUnitAction) trapTriggerAction2, num2 != 0,
                (ProcHitFlags) num3);
        }

        private void RemoveInvalidTargets()
        {
            if (this.m_targets == null)
                return;
            foreach (KeyValuePair<Unit, Aura> keyValuePair in this.m_targets
                .Where<KeyValuePair<Unit, Aura>>((Func<KeyValuePair<Unit, Aura>, bool>) (target =>
                    !target.Key.IsInRadius(this.m_holder, this.m_radius))).ToArray<KeyValuePair<Unit, Aura>>())
            {
                if (keyValuePair.Value.IsAdded && keyValuePair.Key.Auras != null)
                {
                    if (!keyValuePair.Key.IsInContext && keyValuePair.Key.IsInWorld)
                    {
                        Aura aura = keyValuePair.Value;
                        keyValuePair.Key.AddMessage((Action) (() =>
                        {
                            if (!aura.IsAdded)
                                return;
                            aura.Remove(false);
                        }));
                    }
                    else
                        keyValuePair.Value.Remove(false);
                }

                this.m_targets.Remove(keyValuePair.Key);
            }
        }

        /// <summary>Applies this AreaAura's effects to the given target</summary>
        protected void ApplyAuraEffects(Unit target)
        {
            bool flag = this.m_spell.IsBeneficialFor(this.m_CasterReference, (WorldObject) target);
            if (SpellCast.CheckDebuffResist(target, this.m_spell, this.m_CasterReference.Level, !flag) !=
                CastMissReason.None)
                return;
            Aura aura = target.Auras.CreateAura(this.m_CasterReference, this.m_spell, (Item) null);
            if (aura == null)
                return;
            aura.Start(this.m_controller, false);
            this.m_targets.Add(target, aura);
        }

        /// <summary>Removes all auras from the given targets</summary>
        protected static void RemoveEffects(IEnumerable<KeyValuePair<Unit, Aura>> targets)
        {
            foreach (KeyValuePair<Unit, Aura> target in targets)
                target.Value.Remove(false);
        }

        public void Update(int dt)
        {
            if (this.m_timer == null)
                return;
            this.m_timer.Update(dt);
        }
    }
}