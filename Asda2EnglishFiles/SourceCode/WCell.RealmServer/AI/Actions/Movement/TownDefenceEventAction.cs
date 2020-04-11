using NLog;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class TownDefenceEventAction : AIAction
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private int _pointNum = 1;

        public TownDefenceEventAction(Unit owner)
            : base(owner)
        {
        }

        public override void Start()
        {
            this.Update();
        }

        public override void Update()
        {
            this.m_owner.Movement.Update();
            if (this.Owner.Map.DefenceTownEvent == null)
            {
                this.Owner.Health = 0;
            }
            else
            {
                if ((double) this.m_owner.Position.GetDistance(this.Owner.Brain.MovingPoints[this._pointNum]) < 3.0)
                    ++this._pointNum;
                if (this.m_owner.IsMoving)
                    return;
                if (this.Owner.Brain.MovingPoints.Count <= this._pointNum)
                {
                    Map map = this.Owner.Map;
                    int num = 1;
                    switch (((NPC) this.Owner).Entry.Rank)
                    {
                        case CreatureRank.Normal:
                            num = 1;
                            break;
                        case CreatureRank.Elite:
                            num = 3;
                            break;
                        case CreatureRank.Boss:
                            num = 10;
                            break;
                        case CreatureRank.WorldBoss:
                            num = 30;
                            break;
                    }

                    if (map.DefenceTownEvent != null)
                        map.DefenceTownEvent.SubstractPoints(num);
                    this.Owner.Health = 0;
                }
                else
                    this.m_owner.Movement.MoveTo(this.Owner.Brain.MovingPoints[this._pointNum], true);
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