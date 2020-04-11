using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.AI.Actions.Combat
{
    /// <summary>Attack with the main weapon</summary>
    public class AIAttackAction : AITargetMoveAction
    {
        protected float minDist;
        protected float maxDist;
        protected float desiredDist;

        public AIAttackAction(NPC owner)
            : base((Unit) owner)
        {
            this.minDist = 0.0f;
        }

        public override float DistanceMin
        {
            get { return this.minDist; }
        }

        public override float DistanceMax
        {
            get { return this.maxDist; }
        }

        public override float DesiredDistance
        {
            get { return this.desiredDist; }
        }

        /// <summary>Called when starting to attack a new Target</summary>
        public override void Start()
        {
            this.m_owner.IsFighting = true;
            if (this.UsesSpells)
                ((NPC) this.m_owner).NPCSpells.ShuffleReadySpells();
            this.m_target = this.m_owner.Target;
            if (this.m_target != null)
            {
                this.maxDist = this.m_owner.GetBaseAttackRange(this.m_target) - 1f;
                if ((double) this.maxDist < 0.5)
                    this.maxDist = 0.5f;
                this.desiredDist = this.maxDist / 2f;
            }

            if (!this.m_owner.CanMelee)
                return;
            base.Start();
        }

        /// <summary>Called during every Brain tick</summary>
        public override void Update()
        {
            if (this.Target is Character && this.UsesSpells && (this.HasSpellReady && this.m_owner.CanCastSpells) &&
                (this.m_owner.CastingTill < DateTime.Now && this.TryCastSpell()))
            {
                this.m_owner.Movement.Stop();
            }
            else
            {
                if (!this.m_owner.CanMelee || !(this.m_owner.CastingTill < DateTime.Now))
                    return;
                base.Update();
            }
        }

        /// <summary>Called when we stop attacking a Target</summary>
        public override void Stop()
        {
            this.m_owner.IsFighting = false;
            base.Stop();
        }

        /// <summary>
        /// Tries to cast a Spell that is ready and allowed in the current context.
        /// </summary>
        /// <returns></returns>
        protected bool TryCastSpell()
        {
            if (this.Target == null)
                return false;
            NPC owner = (NPC) this.m_owner;
            foreach (Spell readySpell in owner.NPCSpells.ReadySpells)
            {
                if (readySpell.CheckCasterConstraints((Unit) owner) == SpellFailedReason.Ok &&
                    owner.CastingTill < DateTime.Now)
                {
                    owner.CastingTill = DateTime.Now.AddMilliseconds((double) readySpell.CastDelay);
                    return this.m_owner.SpellCast.Start(readySpell, this.Target) == SpellFailedReason.Ok;
                }
            }

            return false;
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.Active; }
        }
    }
}