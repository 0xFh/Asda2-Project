using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    /// <summary>
    /// Lets the owner stay in a specific angle and distance towards a Target
    /// </summary>
    public class AIOrientedTargetMoveAction : AITargetMoveAction
    {
        public AIOrientedTargetMoveAction(Unit owner)
            : base(owner)
        {
        }

        /// <summary>
        /// Whether the Owner should have the same orientation as the Target
        /// </summary>
        public bool SameOrientation { get; set; }

        public float MinAngle { get; set; }

        public float MaxAngle { get; set; }

        public float DesiredAngle
        {
            get { return this.MinAngle + (float) (((double) this.MaxAngle - (double) this.MinAngle) / 2.0); }
        }

        public override void Start()
        {
            if (this.m_target != null)
                return;
            this.m_target = this.m_owner.Target;
            if (this.m_target != null)
                return;
            AITargetMoveAction.log.Error(this.GetType().Name + " is being started without a Target set: " +
                                         (object) this.m_owner);
            this.m_owner.Brain.EnterDefaultState();
        }

        protected override void OnArrived()
        {
            base.OnArrived();
            if (!this.SameOrientation)
                return;
            this.m_owner.Face(this.m_target.Orientation);
        }

        public override bool IsInRange(WorldObject target)
        {
            float angleTowards = this.m_owner.GetAngleTowards((IHasPosition) target);
            if ((double) angleTowards >= (double) this.MinAngle && (double) angleTowards <= (double) this.MaxAngle)
                return base.IsInRange(target);
            return false;
        }

        protected override void MoveToTargetPoint()
        {
            Unit target = this.m_owner.Target;
            float num = target.BoundingRadius + this.m_owner.BoundingRadius;
            Vector3 pos;
            target.GetPointXY(this.DesiredAngle, num + this.DesiredDistance, out pos);
            this.m_owner.Movement.MoveTo(pos, true);
        }

        /// <summary>
        /// Creates a Movement action that lets the owner stay behind its Target
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static AIOrientedTargetMoveAction CreateStayBehindAction(Unit owner)
        {
            return new AIOrientedTargetMoveAction(owner)
            {
                MinAngle = 2.094395f,
                MaxAngle = 4.18879f
            };
        }

        /// <summary>
        /// Creates a Movement action that lets the owner stay in front of its Target
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static AIOrientedTargetMoveAction CreateStayInFrontAction(Unit owner)
        {
            return new AIOrientedTargetMoveAction(owner)
            {
                MinAngle = 5.235988f,
                MaxAngle = 1.047198f
            };
        }
    }
}