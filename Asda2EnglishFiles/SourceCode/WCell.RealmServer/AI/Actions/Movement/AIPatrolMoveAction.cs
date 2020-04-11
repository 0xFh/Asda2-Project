using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIPatrolMoveAction : AIAction
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private List<Vector3> Points = new List<Vector3>();

        public AIPatrolMoveAction(Unit owner, Vector3 pos)
            : base(owner)
        {
            for (float num1 = -10f; (double) num1 <= 10.0; ++num1)
            {
                float num2 = (float) Math.Sqrt(100.0 - Math.Pow((double) num1, 2.0));
                this.Points.Add(new Vector3(pos.X + num1, pos.Y + num2, 0.0f));
            }

            for (float num1 = -6f; (double) num1 <= 6.0; ++num1)
            {
                float num2 = -(float) Math.Sqrt(100.0 - Math.Pow((double) num1, 2.0));
                this.Points.Add(new Vector3(pos.X + num1, pos.Y + num2, 0.0f));
            }
        }

        public override void Start()
        {
            this.Update();
        }

        public override void Update()
        {
            if (!this.m_owner.Movement.Update() && !this.m_owner.CanMove || this.m_owner.IsMoving)
                return;
            this.m_owner.Movement.MoveToPoints(this.Points);
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