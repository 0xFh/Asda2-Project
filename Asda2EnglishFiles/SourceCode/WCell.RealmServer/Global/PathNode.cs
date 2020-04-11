using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Taxi;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Represents a Node in the Taxi-network (can also be considered a "station")
    /// </summary>
    public class PathNode : IWorldLocation, IHasPosition
    {
        /// <summary>All Paths from this Node to its neighbour Nodes</summary>
        public readonly List<TaxiPath> Paths = new List<TaxiPath>();

        public uint Id;
        public MapId mapId;
        public string Name;
        public NPCId HordeMountId;
        public NPCId AllianceMountId;

        public Vector3 Position { get; set; }

        public uint Phase
        {
            get { return uint.MaxValue; }
        }

        public MapId MapId
        {
            get { return this.mapId; }
        }

        public Map Map
        {
            get { return WCell.RealmServer.Global.World.GetNonInstancedMap(this.mapId); }
        }

        public void AddPath(TaxiPath path)
        {
            this.Paths.Add(path);
        }

        public TaxiPath GetPathTo(PathNode toNode)
        {
            foreach (TaxiPath path in this.Paths)
            {
                if (path.To == toNode)
                    return path;
            }

            return (TaxiPath) null;
        }

        public override string ToString()
        {
            return this.Name + " in " + (object) this.MapId + " (" + (object) this.Id + ")";
        }
    }
}