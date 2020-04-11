using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveThenEnterAction : AIAction
    {
        public AIMoveThenEnterAction(Unit owner)
            : this(owner, BrainState.Idle)
        {
        }

        public AIMoveThenEnterAction(Unit owner, BrainState arrivedState)
            : base(owner)
        {
            this.ArrivedState = arrivedState;
        }

        /// <summary>The State to switch to, once arrived</summary>
        public BrainState ArrivedState { get; set; }

        public override void Start()
        {
        }

        public override void Update()
        {
            if (!this.m_owner.Movement.Update())
                return;
            this.m_owner.Brain.State = this.ArrivedState;
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