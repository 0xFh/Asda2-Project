using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIPatrolMoveAction : AIAction
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

       
        List<Vector3> Points = new List<Vector3>();



        public AIPatrolMoveAction(Unit owner, Vector3 pos)
            : base(owner)
        {
            for (float x = -10; x <= 10; x++)
            {
                var y = (float)Math.Sqrt(100 - Math.Pow(x, 2));
                Points.Add(new Vector3(pos.X + x, pos.Y + y, 0));
            }
            for (float x = -6; x <= 6; x ++)
            {
                var y = -(float)Math.Sqrt(100 - Math.Pow(x, 2));
                Points.Add(new Vector3(pos.X + x, pos.Y + y, 0));
            }
        }

        public override void Start()
        {
            Update();
        }

        public override void Update()
        {
            

            if (!m_owner.Movement.Update() && !m_owner.CanMove)
            {
                return;
            }
            if(!m_owner.IsMoving )
                m_owner.Movement.MoveToPoints(Points);
        }


        public override void Stop()
        {
            m_owner.Movement.Stop();
        }


        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }
    }
}