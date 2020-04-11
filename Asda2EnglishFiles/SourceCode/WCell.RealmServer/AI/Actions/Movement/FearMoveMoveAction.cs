using NLog;
using System.Linq;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class FearMoveMoveAction : AIAction
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private Character _gmChar;

        public FearMoveMoveAction(Unit owner, Vector3 pos)
            : base(owner)
        {
            this._gmChar = owner.GetObjectsInRadius<Unit>(200f, ObjectTypes.Player, false, int.MaxValue)
                .FirstOrDefault<WorldObject>() as Character;
        }

        public override void Start()
        {
            this.Update();
        }

        public override void Update()
        {
            this.m_owner.Movement.Update();
            if (this._gmChar == null || this._gmChar.Map != this.Owner.Map)
            {
                this._gmChar = (Character) null;
                this.Owner.Brain.State = BrainState.Combat;
            }
            else
            {
                if (this.m_owner.IsMoving)
                    return;
                Vector3 vector3 = this._gmChar.Position - this.m_owner.Position;
                vector3.Normalize();
                this.m_owner.Movement.MoveTo(this.m_owner.Position - vector3 * 20f, true);
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