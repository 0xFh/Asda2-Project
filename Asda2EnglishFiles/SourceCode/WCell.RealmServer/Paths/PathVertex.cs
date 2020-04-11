using System;
using System.Collections.Generic;
using WCell.Constants.Pathing;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Taxi;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Paths
{
    /// <summary>
    /// A bend or other kind of special location on a <see cref="T:WCell.RealmServer.Taxi.TaxiPath" />
    /// </summary>
    public class PathVertex : IPathVertex, IHasPosition
    {
        internal LinkedListNode<PathVertex> ListEntry = new LinkedListNode<PathVertex>(new PathVertex());
        public uint PathId;
        public uint NodeIndex;
        public MapId MapId;
        public Vector3 Pos;
        public TaxiPathNodeFlags Flags;

        /// <summary>Delay in seconds of how long to stay at this node</summary>
        public uint Delay;

        public uint ArrivalEventId;
        public uint DepartureEventId;
        public float DistFromStart;
        public float DistFromPrevious;

        /// <summary>Normalized vector in direction from the last node</summary>
        public Vector3 FromLastNode;

        /// <summary>Time from start in millis</summary>
        public int TimeFromStart;

        /// <summary>Time from previous node in millis</summary>
        public int TimeFromPrevious;

        public bool HasMapChange;
        public TaxiPath Path;

        public uint Id { get; set; }

        public bool IsStoppingPoint
        {
            get { return this.Flags.HasFlag((Enum) TaxiPathNodeFlags.ArrivalOrDeparture); }
        }

        public float Orientation
        {
            get { return 0.0f; }
        }

        public Vector3 Position
        {
            get { return this.Pos; }
        }

        public uint WaitTime
        {
            get { return this.Delay; }
        }

        public float GetDistanceToNext()
        {
            if (this.ListEntry.Next == null)
                return 0.0f;
            return this.ListEntry.Next.Value.DistFromPrevious;
        }
    }
}