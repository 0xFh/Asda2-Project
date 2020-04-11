using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    public class Movement
    {
        private const float SPEED_FACTOR = 0.001f;

        /// <summary>Starting time of movement</summary>
        protected uint m_lastMoveTime;

        /// <summary>Total time of movement</summary>
        protected uint m_totalMovingTime;

        /// <summary>Time at which movement should end</summary>
        protected uint m_desiredEndMovingTime;

        /// <summary>The target of the current (or last) travel</summary>
        protected Vector3 m_destination;

        protected Path _currentPath;

        /// <summary>The movement type (walking, running or flying)</summary>
        protected AIMoveType m_moveType;

        protected internal Unit m_owner;
        protected bool m_moving;
        protected bool m_MayMove;
        protected PathQuery m_currentQuery;
        private Vector2 _lastDestPosition;

        public Movement(Unit owner)
            : this(owner, AIMoveType.Run)
        {
        }

        public Movement(Unit owner, AIMoveType moveType)
        {
            this.m_owner = owner;
            this.m_moveType = moveType;
            this.m_MayMove = true;
        }

        public Vector3 Destination
        {
            get { return this.m_destination; }
        }

        /// <summary>AI-controlled Movement setting</summary>
        public bool MayMove
        {
            get { return this.m_MayMove; }
            set { this.m_MayMove = value; }
        }

        public bool IsMoving
        {
            get { return this.m_moving; }
        }

        /// <summary>Whether the owner is within 1 yard of the Destination</summary>
        public bool IsAtDestination
        {
            get { return (double) this.m_owner.Position.DistanceSquared(ref this.m_destination) < 1.0; }
        }

        /// <summary>Get movement flags for the packet</summary>
        /// <returns></returns>
        public virtual MonsterMoveFlags MovementFlags
        {
            get
            {
                MonsterMoveFlags monsterMoveFlags;
                switch (this.m_moveType)
                {
                    case AIMoveType.Walk:
                    case AIMoveType.Run:
                        monsterMoveFlags = MonsterMoveFlags.Walk;
                        break;
                    case AIMoveType.Sprint:
                        monsterMoveFlags = MonsterMoveFlags.Flag_0x2000_FullPoints_1;
                        break;
                    case AIMoveType.Fly:
                        monsterMoveFlags = MonsterMoveFlags.Flag_0x2000_FullPoints_1;
                        break;
                    default:
                        monsterMoveFlags = MonsterMoveFlags.Flag_0x2000_FullPoints_1;
                        break;
                }

                return monsterMoveFlags;
            }
        }

        public AIMoveType MoveType
        {
            get { return this.m_moveType; }
            set { this.m_moveType = value; }
        }

        /// <summary>
        /// Remaining movement time to current Destination (in millis)
        /// </summary>
        public uint RemainingTime
        {
            get
            {
                float num1;
                float num2;
                if (this.m_owner.IsFlying)
                {
                    num1 = this.m_owner.FlightSpeed;
                    num2 = this.m_owner.GetDistance(ref this.m_destination);
                }
                else if (this.m_moveType == AIMoveType.Run)
                {
                    num1 = this.m_owner.RunSpeed;
                    num2 = this.m_owner.GetDistanceXY(ref this.m_destination);
                }
                else
                {
                    num1 = this.m_owner.WalkSpeed;
                    num2 = this.m_owner.GetDistanceXY(ref this.m_destination);
                }

                float num3 = num1 * (1f / 1000f);
                return (uint) ((double) num2 / (double) num3);
            }
        }

        /// <summary>Starts the MovementAI</summary>
        /// <returns>Whether already arrived</returns>
        public bool MoveTo(Vector3 destination, bool findPath = true)
        {
            if (!this.m_owner.IsInWorld)
            {
                this.m_owner.DeleteNow();
                return false;
            }

            this.m_destination = destination;
            if (this.IsAtDestination)
                return true;
            if (findPath)
            {
                Vector3 position = this.m_owner.Position;
                position.Z += 5f;
                this.m_currentQuery = new PathQuery(position, ref destination, this.m_owner.ContextHandler,
                    new PathQuery.PathQueryCallback(this.OnPathQueryReply));
                this.m_owner.Map.Terrain.FindPath(this.m_currentQuery);
            }
            else if (this.m_owner.CanMove)
                this.MoveToDestination();

            return false;
        }

        /// <summary>Starts the MovementAI</summary>
        /// <returns>Whether already arrived</returns>
        public bool MoveToPoints(List<Vector3> points)
        {
            if (!this.m_owner.IsInWorld)
            {
                this.m_owner.DeleteNow();
                return false;
            }

            this.m_destination = points[points.Count - 1];
            if (this.IsAtDestination)
                return true;
            Vector3 position = this.m_owner.Position;
            position.Z += 5f;
            this.m_currentQuery = new PathQuery(position, ref this.m_destination, this.m_owner.ContextHandler,
                new PathQuery.PathQueryCallback(this.OnPathQueryReply));
            this.m_currentQuery.Path.Reset(points.Count);
            foreach (Vector3 point in points)
                this.m_currentQuery.Path.Add(point);
            this.m_currentQuery.Reply();
            return false;
        }

        /// <summary>Interpolates the current Position</summary>
        /// <returns>Whether we arrived</returns>
        public bool Update()
        {
            if (!this.m_moving)
                return false;
            if (!this.MayMove)
            {
                this.Stop();
                return false;
            }

            return this.UpdatePosition() && !this.CheckCollision();
        }

        private bool CheckCollision()
        {
            IList<WorldObject> objectsInRadius =
                this.m_owner.GetObjectsInRadius<Unit>(this.m_owner.BoundingCollisionRadius, ObjectTypes.Unit, false,
                    int.MaxValue);
            objectsInRadius.Remove((WorldObject) this.m_owner);
            float x = 0.0f;
            float y = 0.0f;
            foreach (Unit unit in (IEnumerable<WorldObject>) objectsInRadius)
            {
                float num = this.m_owner.IsCollisionWith(unit);
                if ((double) num > 0.0)
                {
                    Vector3 vector3 = new Vector3(this.m_owner.Position.X - unit.Position.X,
                        this.m_owner.Position.Y - unit.Position.Y);
                    vector3.Normalize();
                    x += vector3.X * num;
                    y += vector3.Y * num;
                }
            }

            Vector3 vector3_1 = new Vector3(x, y);
            if (!(vector3_1 != Vector3.Zero))
                return false;
            if ((double) vector3_1.Length() > (double) this.m_owner.BoundingRadius / 2.0)
            {
                this.m_destination = new Vector3(this.m_owner.Position.X + x, this.m_owner.Position.Y + y);
                this.MoveToDestination();
            }

            return true;
        }

        /// <summary>Stops at the current position</summary>
        public void Stop()
        {
            if (!this.m_moving)
                return;
            this.UpdatePosition();
            this.m_moving = false;
        }

        /// <summary>Starts moving to current Destination</summary>
        /// <remarks>Sends movement packet to client</remarks>
        protected void MoveToDestination()
        {
            this.m_moving = true;
            this.m_totalMovingTime = this.RemainingTime;
            this.m_owner.SetOrientationTowards(ref this.m_destination);
            NPC owner = this.m_owner as NPC;
            if (this._lastDestPosition != this.m_destination.XY)
            {
                Asda2MovmentHandler.SendMonstMoveOrAtackResponse((short) -1, owner, -1,
                    new Vector3(this.m_destination.X - this.m_owner.Map.Offset,
                        this.m_destination.Y - this.m_owner.Map.Offset), false);
                this._lastDestPosition = this.m_destination.XY;
            }

            this.m_lastMoveTime = Utility.GetSystemTime();
            this.m_desiredEndMovingTime = this.m_lastMoveTime + this.m_totalMovingTime;
        }

        protected void OnPathQueryReply(PathQuery query)
        {
            if (query != this.m_currentQuery)
                return;
            this.m_currentQuery = (PathQuery) null;
            this.FollowPath(query.Path);
        }

        public void FollowPath(Path path)
        {
            this._currentPath = path;
            this.m_destination = this._currentPath.Next();
            this.MoveToDestination();
        }

        /// <summary>Updates position of unit</summary>
        /// <returns>true if target point is reached</returns>
        protected bool UpdatePosition()
        {
            uint systemTime = Utility.GetSystemTime();
            float num = (float) (systemTime - this.m_lastMoveTime) / (float) this.m_totalMovingTime;
            if (systemTime >= this.m_desiredEndMovingTime || (double) num >= 1.0)
            {
                this.m_owner.Map.MoveObject((WorldObject) this.m_owner, ref this.m_destination);
                if (this._currentPath != null)
                {
                    if (this._currentPath.HasNext())
                    {
                        if (!this.CheckCollision())
                        {
                            this.m_destination = this._currentPath.Next();
                            this.MoveToDestination();
                        }
                    }
                    else
                        this._currentPath = (Path) null;

                    return false;
                }

                this.m_moving = false;
                return true;
            }

            Vector3 position = this.m_owner.Position;
            Vector3 newPos = position + (this.m_destination - position) * num;
            this.m_lastMoveTime = systemTime;
            this.m_owner.Map.MoveObject((WorldObject) this.m_owner, ref newPos);
            return false;
        }
    }
}