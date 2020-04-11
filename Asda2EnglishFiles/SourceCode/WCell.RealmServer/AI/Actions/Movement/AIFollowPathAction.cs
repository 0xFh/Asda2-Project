using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIFollowPathAction : AIAction
    {
        private Path _path;

        public AIFollowPathAction(Unit owner, Path path = null, AIMoveType moveType = AIMoveType.Walk)
            : base(owner)
        {
            this.Path = path;
            this.MoveType = moveType;
        }

        public Path Path
        {
            get { return this._path; }
            set { this._path = value; }
        }

        public AIMoveType MoveType { get; set; }

        private void MoveToNext()
        {
            Vector3 destination = this._path.Next();
            this.m_owner.Brain.SourcePoint = destination;
            this.m_owner.Movement.MoveTo(destination, false);
        }

        public override void Start()
        {
            this.m_owner.Movement.MoveType = this.MoveType;
            this.MoveToNext();
        }

        public override void Update()
        {
            if (this.Path == null || !this.m_owner.Movement.Update() || !this._path.HasNext())
                return;
            this.MoveToNext();
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