using System.Linq;
using NLog;
using WCell.Constants.NPCs;
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
            _gmChar =
                owner.GetObjectsInRadius(200, ObjectTypes.Player, false).FirstOrDefault(c => ((Character)c).GodMode) as Character;
            if (_gmChar != null)
            {
                _relativePosition = new Vector3(_gmChar.Position.X - owner.Position.X, _gmChar.Position.Y - owner.Position.Y);
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
            if (_gmChar == null || _gmChar.Map != Owner.Map)
            {
                _gmChar = null;
                Owner.Brain.EnterDefaultState();
                return;
            }

            if (!m_owner.IsMoving)
            {
                var newPos = (_gmChar.Position - _relativePosition);
                var dist = newPos.GetDistance(m_owner.Position);
                if (dist > 1)
                {
                    m_owner.Movement.MoveTo(newPos);
                }
            }
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
    public class FearMoveMoveAction : AIAction
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Character _gmChar;


        public FearMoveMoveAction(Unit owner, Vector3 pos)
            : base(owner)
        {
            _gmChar =
                owner.GetObjectsInRadius(200, ObjectTypes.Player, false).FirstOrDefault() as Character;
        }

        public override void Start()
        {
            Update();
        }

        public override void Update()
        {
            m_owner.Movement.Update();
            if (_gmChar == null || _gmChar.Map != Owner.Map)
            {
                _gmChar = null;
                Owner.Brain.State = BrainState.Combat;
                return;
            }

            if (!m_owner.IsMoving)
            {
                var vector = (_gmChar.Position - m_owner.Position);
                vector.Normalize();

                m_owner.Movement.MoveTo(m_owner.Position - vector * 20);
            }
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
            Update();
        }

        public override void Update()
        {
            m_owner.Movement.Update();

            if (Owner.Map.DefenceTownEvent == null)
            {
                Owner.Health = 0;
                return;
            }
            if (m_owner.Position.GetDistance(Owner.Brain.MovingPoints[_pointNum]) < 3)
                _pointNum++;
            if (!m_owner.IsMoving)
            {
                if (Owner.Brain.MovingPoints.Count <= _pointNum)
                {
                    //Boom
                    var map = Owner.Map;
                    var value = 1;
                    switch (((NPC)Owner).Entry.Rank)
                    {
                            case CreatureRank.Boss:
                            value = 10;
                            break;
                            case CreatureRank.WorldBoss:
                            value = 30;
                            break;
                            case CreatureRank.Elite:
                            value = 3;
                            break;
                            case CreatureRank.Normal:
                            value = 1;
                            break;
                    }
                    if (map.DefenceTownEvent != null)
                    {
                        map.DefenceTownEvent.SubstractPoints(value);
                    }
                    Owner.Health = 0;
                }
                else
                {
                    var point = Owner.Brain.MovingPoints[_pointNum];
                    m_owner.Movement.MoveTo(point);
                }
            }
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