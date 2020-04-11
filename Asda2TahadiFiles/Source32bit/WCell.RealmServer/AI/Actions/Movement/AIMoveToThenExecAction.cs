using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveToThenExecAction : AIMoveToTargetAction
    {
        protected int m_TimeoutTicks;
        protected int m_RuntimeTicks;

        public AIMoveToThenExecAction(Unit owner, UnitActionCallback actionCallback)
            : base(owner)
        {
            this.ActionCallback = actionCallback;
        }

        /// <summary>
        /// The Action to execute, once the Target has been approached
        /// </summary>
        public UnitActionCallback ActionCallback { get; set; }

        public int TimeoutMillis
        {
            get { return this.m_TimeoutTicks / this.m_owner.Map.UpdateDelay; }
            set { this.m_TimeoutTicks = value / this.m_owner.Map.UpdateDelay; }
        }

        public int TimeoutTicks
        {
            get { return this.m_TimeoutTicks; }
            set { this.m_TimeoutTicks = value; }
        }

        public int RuntimeMillis
        {
            get { return this.m_RuntimeTicks / this.m_owner.Map.UpdateDelay; }
            set { this.m_RuntimeTicks = value / this.m_owner.Map.UpdateDelay; }
        }

        public int RuntimeTicks
        {
            get { return this.m_RuntimeTicks; }
            set { this.m_RuntimeTicks = value; }
        }

        public override void Start()
        {
            this.m_RuntimeTicks = 0;
            base.Start();
        }

        public override void Update()
        {
            ++this.m_RuntimeTicks;
            if (this.m_TimeoutTicks > 0 && this.m_RuntimeTicks >= this.m_TimeoutTicks)
            {
                this.Stop();
                this.OnTimeout();
            }
            else
                base.Update();
        }

        public override void Stop()
        {
            this.m_RuntimeTicks = 0;
            base.Stop();
        }

        protected override void OnArrived()
        {
            base.OnArrived();
            if (this.ActionCallback == null)
                return;
            this.ActionCallback(this.m_owner);
        }
    }
}