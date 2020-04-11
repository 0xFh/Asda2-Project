using System.Collections.Generic;
using WCell.RealmServer.Global;
using WCell.RealmServer.Paths;

namespace WCell.RealmServer.Taxi
{
    public class TaxiPath
    {
        public readonly LinkedList<PathVertex> Nodes = new LinkedList<PathVertex>();
        public uint Id;
        public uint StartNodeId;
        public uint EndNodeId;
        public uint Price;
        public PathNode From;
        public PathNode To;
        public float PathLength;

        /// <summary>
        /// The total time that it takes to fly on this Path from start to end
        /// in millis (equals <see cref="F:WCell.RealmServer.Taxi.TaxiPath.PathLength" />*<see cref="P:WCell.RealmServer.Taxi.TaxiMgr.AirSpeed" />)
        /// </summary>
        public uint PathTime;

        /// <summary>
        /// Returns a partial list of TaxiPathNodes
        ///  that comprise the remainder of the Units Taxi flight.
        /// </summary>
        public override string ToString()
        {
            if (this.From != null && this.To != null)
                return string.Format("Path from {0} to {1}", (object) this.From.Name, (object) this.To.Name);
            return string.Empty;
        }
    }
}