using System;
using System.Collections.Generic;
using WCell.Constants.Pathing;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.Core.Terrain.Paths;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Paths;
using WCell.RealmServer.Taxi;
using WCell.RealmServer.Transports;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.Threading;

namespace WCell.RealmServer.Entities
{
    /// <summary>Ships, Zeppelins etc</summary>
    public class Transport : GameObject, ITransportInfo, IFactionMember, IWorldLocation, IHasPosition, INamedEntity,
        IEntity, INamed, IContextHandler
    {
        private List<Unit> m_passengers = new List<Unit>();
        private uint m_pathTime;
        private LinkedList<TransportPathVertex> m_transportPathVertices;
        private LinkedListNode<TransportPathVertex> m_CurrentPathVertex;
        private LinkedListNode<TransportPathVertex> m_NextPathVertex;
        private MapId[] m_mapIds;
        private GOMOTransportEntry m_goTransportEntry;
        private TransportEntry m_transportEntry;
        private bool m_isMOTransport;

        public override UpdateFlags UpdateFlags
        {
            get
            {
                return UpdateFlags.Transport | UpdateFlags.Flag_0x10 | UpdateFlags.StationaryObject |
                       UpdateFlags.HasRotation;
            }
        }

        protected override void WriteMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(this.Position.X);
            writer.Write(this.Position.Y);
            writer.Write(this.Position.Z);
            writer.WriteFloat(this.Orientation);
        }

        protected override void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(150754760);
        }

        public List<Unit> Passengers
        {
            get { return this.m_passengers; }
        }

        public MapId[] MapIds
        {
            get { return this.m_mapIds; }
        }

        protected internal Transport()
        {
            this.AnimationProgress = byte.MaxValue;
            this.m_passengers = new List<Unit>();
        }

        internal override void Init(GOEntry entry, GOSpawnEntry spawnEntry, GOSpawnPoint spawnPoint)
        {
            base.Init(entry, spawnEntry, spawnPoint);
            this.m_goTransportEntry = this.Entry as GOMOTransportEntry;
            TransportMgr.TransportEntries.TryGetValue(this.m_entry.GOId, out this.m_transportEntry);
            this.m_isMOTransport = this.m_goTransportEntry != null && this.m_transportEntry != null;
        }

        public void AddPassenger(Unit unit)
        {
            this.Passengers.Add(unit);
            unit.Transport = this;
        }

        public void RemovePassenger(Unit unit)
        {
            this.Passengers.Remove(unit);
            unit.Transport = (Transport) null;
        }

        internal void GenerateWaypoints(TaxiPath path)
        {
            this.GenerateWaypoints(path, out this.m_mapIds);
        }

        private void GenerateWaypoints(TaxiPath path, out MapId[] mapIds)
        {
            mapIds = (MapId[]) null;
            if (path == null)
                return;
            LinkedListNode<TransportPathTempVertex> pathStart;
            LinkedListNode<TransportPathTempVertex> pathStop;
            LinkedList<TransportPathTempVertex> tempVertices =
                this.GetTempVertices(path, out pathStart, out pathStop, out mapIds);
            this.FillDistFromStop(tempVertices, pathStop);
            this.FillDistToStop(tempVertices, pathStart);
            LinkedListNode<TransportPathTempVertex> linkedListNode1 = tempVertices.First;
            while (linkedListNode1 != null)
                linkedListNode1 = linkedListNode1.Next;
            LinkedListNode<TransportPathTempVertex> linkedListNode2 = tempVertices.First;
            LinkedListNode<TransportPathTempVertex> linkedListNode3 = tempVertices.Last;
            this.m_transportPathVertices = new LinkedList<TransportPathVertex>();
            uint num = 0;
            for (; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
            {
                bool flag = linkedListNode2.Value.Vertex.MapId != linkedListNode3.Value.Vertex.MapId ||
                            linkedListNode2.Value.Vertex.Flags.HasFlag((Enum) TaxiPathNodeFlags.IsTeleport);
                this.m_transportPathVertices.AddLast(new TransportPathVertex()
                {
                    Time = num,
                    MapId = linkedListNode2.Value.Vertex.MapId,
                    Position = linkedListNode2.Value.Vertex.Position,
                    Teleport = flag
                });
                num =
                    ((double) linkedListNode2.Value.MoveTimeFromFirstStop >=
                     (double) linkedListNode2.Value.MoveTimeToLastStop
                        ? num + (uint) ((double) Math.Abs(linkedListNode2.Value.MoveTimeToLastStop -
                                                          linkedListNode3.Value.MoveTimeToLastStop) * 1000.0)
                        : num + (uint) ((double) Math.Abs(linkedListNode2.Value.MoveTimeFromFirstStop -
                                                          linkedListNode3.Value.MoveTimeFromFirstStop) * 1000.0)) +
                    linkedListNode2.Value.Vertex.Delay * 1000U;
                linkedListNode3 = linkedListNode2;
            }

            this.m_pathTime = num;
        }

        private LinkedList<TransportPathTempVertex> GetTempVertices(TaxiPath path,
            out LinkedListNode<TransportPathTempVertex> pathStart, out LinkedListNode<TransportPathTempVertex> pathStop,
            out MapId[] mapIds)
        {
            List<MapId> mapIdList = new List<MapId>();
            pathStop = (LinkedListNode<TransportPathTempVertex>) null;
            pathStart = (LinkedListNode<TransportPathTempVertex>) null;
            LinkedListNode<PathVertex> linkedListNode1 = path.Nodes.First;
            LinkedList<TransportPathTempVertex> linkedList = new LinkedList<TransportPathTempVertex>();
            for (; linkedListNode1 != null; linkedListNode1 = linkedListNode1.Next)
            {
                if (!mapIdList.Contains(linkedListNode1.Value.MapId))
                    mapIdList.Add(linkedListNode1.Value.MapId);
                TransportPathTempVertex transportPathTempVertex =
                    new TransportPathTempVertex(0.0f, 0.0f, 0.0f, linkedListNode1.Value);
                LinkedListNode<TransportPathTempVertex> linkedListNode2 = linkedList.AddLast(transportPathTempVertex);
                if (linkedListNode1.Value.IsStoppingPoint)
                {
                    pathStop = linkedListNode2;
                    if (pathStart == null)
                        pathStart = linkedListNode2;
                }
            }

            if (pathStart == null)
                throw new Exception("TaxiPath provided does not have any stops");
            mapIds = mapIdList.ToArray();
            return linkedList;
        }

        private void FillDistFromStop(LinkedList<TransportPathTempVertex> tempVertices,
            LinkedListNode<TransportPathTempVertex> pathStop)
        {
            LinkedListNode<TransportPathTempVertex> linkedListNode = pathStop;
            float num = 0.0f;
            for (;
                linkedListNode != pathStop && linkedListNode != null;
                linkedListNode = linkedListNode.Next ?? tempVertices.First)
            {
                if (linkedListNode.Value.Vertex.Flags.HasFlag((Enum) TaxiPathNodeFlags.IsTeleport))
                    num = 0.0f;
                else
                    num += linkedListNode.Value.Vertex.DistFromPrevious;
                linkedListNode.Value.DistFromFirstStop = num;
            }
        }

        private void FillDistToStop(LinkedList<TransportPathTempVertex> tempVertices,
            LinkedListNode<TransportPathTempVertex> pathStart)
        {
            LinkedListNode<TransportPathTempVertex> linkedListNode1 = pathStart;
            float num = 0.0f;
            for (;
                linkedListNode1 != pathStart && linkedListNode1 != null;
                linkedListNode1 = linkedListNode1.Previous ?? tempVertices.Last)
            {
                LinkedListNode<TransportPathTempVertex> linkedListNode2 = linkedListNode1.Next ?? tempVertices.First;
                num += linkedListNode2.Value.Vertex.DistFromPrevious;
                linkedListNode1.Value.DistToLastStop = num;
                if (linkedListNode1.Value.Vertex.Flags.HasFlag((Enum) TaxiPathNodeFlags.IsTeleport))
                    num = 0.0f;
            }
        }

        public override void Update(int dt)
        {
            base.Update(dt);
            int num = this.m_isMOTransport ? 1 : 0;
        }

        private void AdvanceByOneWaypoint()
        {
            this.m_CurrentPathVertex = this.m_NextPathVertex;
            this.m_NextPathVertex = this.m_NextPathVertex.Next ?? this.m_transportPathVertices.First;
        }

        private void MoveTransport(MapId mapId, Vector3 position)
        {
            if (this.m_CurrentPathVertex.Value.MapId != this.Map.Id || this.m_CurrentPathVertex.Value.Teleport)
                return;
            this.m_position = this.m_CurrentPathVertex.Value.Position;
        }
    }
}