using System;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveToGameObjectIntoAngleThenExecAction : AIMoveToThenExecAction
    {
        private const float ErrorMargin = 0.5235988f;
        private readonly float _angle;
        private readonly GameObject _gameObject;

        public AIMoveToGameObjectIntoAngleThenExecAction(Unit owner, GameObject go, float angle,
            UnitActionCallback actionCallback)
            : base(owner, actionCallback)
        {
            this._angle = angle;
            this._gameObject = go;
        }

        public float Angle
        {
            get { return this._angle; }
        }

        public override void Start()
        {
            if (this._gameObject == null)
            {
                AITargetMoveAction.log.Error("Started " + this.GetType().Name + " without Target set: " +
                                             (object) this.m_owner);
                this.m_owner.Brain.EnterDefaultState();
            }
            else
                this.Update();
        }

        public override bool IsInRange(WorldObject target)
        {
            float num = Math.Abs(this.m_owner.Orientation - this.m_owner.GetAngleTowards((IHasPosition) target));
            if ((double) num >= (double) this._angle - 0.523598790168762 &&
                (double) num <= (double) this._angle + 0.523598790168762)
                return base.IsInRange(target);
            return false;
        }

        protected override void MoveToTargetPoint()
        {
            Vector3 pos;
            this._gameObject.GetPointXY(this._angle, this.DesiredDistance, out pos);
            pos.Z = this._gameObject.Position.Z;
            this.m_owner.Movement.MoveTo(pos, true);
        }

        public override void Update()
        {
            if (this._gameObject == null || !this._gameObject.IsInWorld)
            {
                this.OnLostTarget();
                if (this._gameObject == null)
                    return;
            }

            if (!this.m_owner.Movement.Update() && !this.m_owner.CanMove)
                return;
            if (!this.m_owner.CanSee((WorldObject) this._gameObject))
                this.m_owner.Movement.Stop();
            if (this.IsInRange((WorldObject) this._gameObject))
            {
                this.OnArrived();
            }
            else
            {
                if (this.m_owner.IsMoving && !this.m_owner.CheckTicks(AITargetMoveAction.UpdatePositionTicks))
                    return;
                this.MoveToTargetPoint();
            }
        }
    }
}