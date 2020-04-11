using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveThenExecAction : AIAction
    {
        public AIMoveThenExecAction(Unit owner, UnitActionCallback actionCallback)
            : base(owner)
        {
            this.ActionCallback = actionCallback;
        }

        /// <summary>The Action to execute, once arrived</summary>
        public UnitActionCallback ActionCallback { get; set; }

        public override void Start()
        {
        }

        public override void Update()
        {
            if (!this.m_owner.Movement.Update() || this.ActionCallback == null)
                return;
            this.ActionCallback(this.m_owner);
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