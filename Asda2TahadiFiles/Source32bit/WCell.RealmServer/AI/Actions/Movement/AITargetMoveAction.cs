using NLog;
using System;
using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    /// <summary>
    /// Lets the owner stay in a specific distance towards a Target
    /// </summary>
    public class AITargetMoveAction : AIAction
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static int UpdatePositionTicks = 4;
        public static float DefaultFollowDistanceMax = 5f;
        public static float DefaultFollowDistanceMin = 0.0f;
        public static float DefaultDesiredDistance = 3f;
        protected Unit m_target;

        public AITargetMoveAction(Unit owner)
            : base(owner)
        {
        }

        public virtual float DistanceMin
        {
            get { return AITargetMoveAction.DefaultFollowDistanceMin; }
        }

        public virtual float DistanceMax
        {
            get { return AITargetMoveAction.DefaultFollowDistanceMax; }
        }

        public virtual float DesiredDistance
        {
            get { return AITargetMoveAction.DefaultDesiredDistance; }
        }

        public Unit Target
        {
            get { return this.m_target; }
            set { this.m_target = value; }
        }

        public virtual bool IsInRange(WorldObject target)
        {
            NPC owner = (NPC) this.m_owner;
            float distanceSq = this.m_owner.GetDistanceSq(target);
            foreach (Spell readySpell in owner.NPCSpells.ReadySpells)
            {
                if ((double) readySpell.Range.MaxDist * (double) readySpell.Range.MaxDist > (double) distanceSq)
                    return true;
            }

            return (double) distanceSq < (double) this.DistanceMax * (double) this.DistanceMax;
        }

        public override void Start()
        {
            if (this.m_target == null)
            {
                if (this.m_owner.Target == null)
                {
                    AITargetMoveAction.log.Error("Started " + this.GetType().Name + " without Target set: " +
                                                 (object) this.m_owner);
                    this.m_owner.Brain.EnterDefaultState();
                    return;
                }

                this.m_target = this.m_owner.Target;
            }

            this.Update();
        }

        public override void Update()
        {
            if (this.m_owner.CastingTill > DateTime.Now)
                return;
            if (this.m_target == null || !this.m_target.IsInWorld || this.m_target.Map != this.m_owner.Map)
            {
                this.OnLostTarget();
                if (this.m_target == null)
                    return;
            }

            if (!this.m_owner.Movement.Update() && !this.m_owner.CanMove)
                return;
            if (!this.m_owner.CanSee((WorldObject) this.m_target))
                this.m_owner.Movement.Stop();
            if (this.UsesSpells && this.HasSpellReady &&
                (this.m_owner.CanCastSpells && this.m_owner.NextSpellUpdate < DateTime.Now))
            {
                this.m_owner.NextSpellUpdate = DateTime.Now.AddMilliseconds(CharacterFormulas.NpcSpellUpdateDelay);
                this.OnArrived();
            }
            else if (this.IsInRange((WorldObject) this.m_target))
            {
                this.OnArrived();
            }
            else
            {
                if (this.m_owner.IsMoving && !(this.NextUpdate < DateTime.Now))
                    return;
                this.MoveToTargetPoint();
            }
        }

        protected virtual void OnLostTarget()
        {
            this.Stop();
            this.m_owner.Brain.EnterDefaultState();
        }

        protected virtual void OnArrived()
        {
            this.m_owner.Movement.Stop();
            if (this.m_target == null)
                return;
            this.m_owner.SetOrientationTowards((IHasPosition) this.m_target);
        }

        protected virtual void OnTimeout()
        {
            this.m_owner.Brain.EnterDefaultState();
        }

        public override void Stop()
        {
            this.m_owner.Movement.Stop();
            this.m_target = (Unit) null;
        }

        /// <summary>
        /// Gets a preferred point, close to the current target and walks towards it
        /// </summary>
        /// <returns></returns>
        protected virtual void MoveToTargetPoint()
        {
            float num = this.m_target.BoundingRadius + this.m_owner.BoundingRadius;
            Vector3 vector3 = this.m_target.Position - this.m_owner.Position;
            if (vector3 == Vector3.Zero)
                vector3 = Vector3.Right;
            else
                vector3.Normalize();
            this.m_owner.Movement.MoveTo(this.m_target.Position - vector3 * (this.DesiredDistance + num), true);
            this.NextUpdate = DateTime.Now.AddMilliseconds((double) CharacterFormulas.NpcMoveUpdateDelay);
        }

        protected DateTime NextUpdate { get; set; }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }
    }
}