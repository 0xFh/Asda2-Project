using System;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveIntoAngleThenExecAction : AIMoveToThenExecAction
    {
        private const float ErrorMargin = 0.5235988f;
        private readonly float m_Angle;

        public AIMoveIntoAngleThenExecAction(Unit owner, float angle, UnitActionCallback actionCallback)
            : base(owner, actionCallback)
        {
            this.m_Angle = angle;
        }

        public float Angle
        {
            get { return this.m_Angle; }
        }

        public override bool IsInRange(WorldObject target)
        {
            float num = Math.Abs(this.m_owner.Orientation - this.m_owner.GetAngleTowards((IHasPosition) target));
            if ((double) num >= (double) this.m_Angle - 0.523598790168762 &&
                (double) num <= (double) this.m_Angle + 0.523598790168762)
                return base.IsInRange(target);
            return false;
        }

        protected override void MoveToTargetPoint()
        {
            Unit target = this.m_owner.Target;
            float num = target.BoundingRadius + this.m_owner.BoundingRadius;
            Vector3 pos;
            target.GetPointXY(this.m_Angle, num + this.DesiredDistance, out pos);
            this.m_owner.Movement.MoveTo(pos, true);
        }
    }
}