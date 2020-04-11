using NLog;
using System;
using System.Linq;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIGMMoveMoveAction : AIAction
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private Character _gmChar;
        private Vector3 _relativePosition;

        public AIGMMoveMoveAction(Unit owner, Vector3 pos)
            : base(owner)
        {
            this._gmChar = owner.GetObjectsInRadius<Unit>(200f, ObjectTypes.Player, false, int.MaxValue)
                .FirstOrDefault<WorldObject>((Func<WorldObject, bool>) (c => ((Character) c).GodMode)) as Character;
            if (this._gmChar == null)
                return;
            this._relativePosition = new Vector3(this._gmChar.Position.X - owner.Position.X,
                this._gmChar.Position.Y - owner.Position.Y);
        }

        public override void Start()
        {
            this.Update();
        }

        public override void Update()
        {
            if (!this.m_owner.Movement.Update() && !this.m_owner.CanMove)
                return;
            if (this._gmChar == null || this._gmChar.Map != this.Owner.Map)
            {
                this._gmChar = (Character) null;
                this.Owner.Brain.EnterDefaultState();
            }
            else
            {
                if (this.m_owner.IsMoving)
                    return;
                Vector3 destination = this._gmChar.Position - this._relativePosition;
                if ((double) destination.GetDistance(this.m_owner.Position) <= 1.0)
                    return;
                this.m_owner.Movement.MoveTo(destination, true);
            }
        }

        public override void Stop()
        {
            this.m_owner.Movement.Stop();
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }
    }
}