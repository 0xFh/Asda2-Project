using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveIntoRangeOfGOThenExecAction : AIMoveToThenExecAction
    {
        private SimpleRange m_Range;
        private GameObject _gameObject;

        public AIMoveIntoRangeOfGOThenExecAction(Unit owner, GameObject go, SimpleRange range,
            UnitActionCallback actionCallback)
            : base(owner, actionCallback)
        {
            this._gameObject = go;
            this.m_Range = range;
        }

        public override float DistanceMin
        {
            get { return this.m_Range.MinDist; }
        }

        public override float DistanceMax
        {
            get { return this.m_Range.MaxDist; }
        }

        public override float DesiredDistance
        {
            get { return this.m_Range.Average; }
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

        /// <summary>
        /// Gets a preferred point, close to the current target and walks towards it
        /// </summary>
        /// <returns></returns>
        protected override void MoveToTargetPoint()
        {
            Vector3 vector3 = this._gameObject.Position - this.m_owner.Position;
            if (vector3 == Vector3.Zero)
                vector3 = Vector3.Right;
            else
                vector3.Normalize();
            this.m_owner.Movement.MoveTo(this._gameObject.Position - vector3 * this.DesiredDistance, true);
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