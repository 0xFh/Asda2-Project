using System;
using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util;

namespace WCell.RealmServer.AI.Actions.Movement
{
    /// <summary>
    /// Movement AI action for moving through a set of waypoints
    /// </summary>
    public class AIWaypointMoveAction : AIAction
    {
        protected LinkedList<WaypointEntry> Waypoints;
        protected LinkedListNode<WaypointEntry> CurrentWaypoint;
        protected LinkedListNode<WaypointEntry> TargetWaypoint;

        /// <summary>
        /// The direction of movement on waypoints. True - moving backwards, false - moving forward
        /// Only used for AIMovementType.ForwardThenBack
        /// </summary>
        protected bool GoingBack;

        /// <summary>Whether we are staying on a waypoint (pause)</summary>
        protected bool StayingOnWaypoint;

        /// <summary>When to start moving again</summary>
        protected uint DesiredStartMovingTime;

        protected AIMovementType WaypointSequence;

        public AIWaypointMoveAction(Unit owner)
            : this(owner, AIMovementType.ForwardThenStop)
        {
        }

        public AIWaypointMoveAction(Unit owner, AIMovementType waypointSequence)
            : base(owner)
        {
            this.Waypoints = new LinkedList<WaypointEntry>();
            this.WaypointSequence = waypointSequence;
        }

        public AIWaypointMoveAction(Unit owner, AIMovementType waypointSequence, LinkedList<WaypointEntry> waypoints)
            : this(owner, waypointSequence)
        {
            this.Waypoints = waypoints ?? WaypointEntry.EmptyList;
        }

        /// <summary>Amount of Waypoints</summary>
        public int Count
        {
            get { return this.Waypoints.Count; }
        }

        public bool IsStayingOnWaypoint
        {
            get { return this.StayingOnWaypoint; }
        }

        public override void Start()
        {
            this.StayingOnWaypoint = true;
            this.DesiredStartMovingTime = 0U;
            this.m_owner.Movement.MoveType = AIMoveType.Walk;
        }

        public override void Update()
        {
            if (this.Waypoints.Count == 0)
                return;
            if (this.StayingOnWaypoint)
            {
                if (Utility.GetSystemTime() < this.DesiredStartMovingTime)
                    return;
                this.TargetWaypoint = this.GetNextWaypoint();
                this.MoveToTargetWaypoint();
            }
            else
            {
                if (!this.m_owner.Movement.Update())
                    return;
                this.CurrentWaypoint = this.TargetWaypoint;
                if ((double) Math.Abs(this.CurrentWaypoint.Value.Orientation - 0.0f) > 1.40129846432482E-45)
                    this.m_owner.Face(this.CurrentWaypoint.Value.Orientation);
                uint waitTime = this.TargetWaypoint.Value.WaitTime;
                this.StayingOnWaypoint = true;
                this.DesiredStartMovingTime = Utility.GetSystemTime() + waitTime;
            }
        }

        public override void Stop()
        {
            this.m_owner.Movement.Stop();
        }

        protected void MoveToTargetWaypoint()
        {
            if (this.TargetWaypoint == null)
                return;
            this.StayingOnWaypoint = false;
            this.m_owner.Brain.SourcePoint = this.TargetWaypoint.Value.Position;
            this.m_owner.Movement.MoveTo(this.TargetWaypoint.Value.Position, false);
        }

        protected LinkedListNode<WaypointEntry> GetNextWaypoint()
        {
            if (this.Waypoints.Count == 0)
                return (LinkedListNode<WaypointEntry>) null;
            if (this.CurrentWaypoint == null)
                return this.Waypoints.First;
            switch (this.WaypointSequence)
            {
                case AIMovementType.ForwardThenBack:
                    if (!this.GoingBack)
                    {
                        if (this.CurrentWaypoint.Next != null)
                            return this.CurrentWaypoint.Next;
                        if (this.CurrentWaypoint.Previous != null)
                        {
                            this.GoingBack = true;
                            return this.CurrentWaypoint.Previous;
                        }
                    }

                    if (this.GoingBack)
                    {
                        if (this.CurrentWaypoint.Previous != null)
                            return this.CurrentWaypoint.Previous;
                        if (this.CurrentWaypoint.Next != null)
                        {
                            this.GoingBack = false;
                            return this.CurrentWaypoint.Next;
                        }
                    }

                    return (LinkedListNode<WaypointEntry>) null;
                case AIMovementType.ForwardThenStop:
                    return this.CurrentWaypoint.Next;
                case AIMovementType.ForwardThenFirst:
                    if (this.CurrentWaypoint.Next != null)
                        return this.CurrentWaypoint.Next;
                    return this.Waypoints.First;
                default:
                    return (LinkedListNode<WaypointEntry>) null;
            }
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }
    }
}