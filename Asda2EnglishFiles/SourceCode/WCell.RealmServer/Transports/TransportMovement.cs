using System;
using System.Collections.Generic;
using WCell.Constants.World;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Paths;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Transports
{
    public class TransportMovement
    {
        private GOMOTransportEntry m_entry;
        private LinkedListNode<PathVertex>[] m_stops;
        private float m_accelerationRate;
        private float m_moveSpeed;
        private float m_accelerationTime;
        private float m_accelerationDist;

        public TransportMovement(GOMOTransportEntry entry, uint period)
        {
            this.m_entry = entry;
            this.Initialize();
        }

        public MapId GetMap(int time)
        {
            return MapId.EasternKingdoms;
        }

        public Vector3 GetPosition(int time)
        {
            return Vector3.Zero;
        }

        private void Initialize()
        {
            this.InitializeConstants();
            this.FindStopsAndMapIds(this.m_entry.Path.Nodes.First);
            if (this.m_stops.Length != 2)
                return;
            LinkedListNode<PathVertex> stop1 = this.m_stops[0];
            LinkedListNode<PathVertex> stop2 = this.m_stops[1];
            this.AppendAccelerationNodes(stop1);
        }

        private void FindStopsAndMapIds(LinkedListNode<PathVertex> node)
        {
            List<MapId> mapIdList = new List<MapId>();
            List<LinkedListNode<PathVertex>> linkedListNodeList = new List<LinkedListNode<PathVertex>>();
            for (; node != null; node = node.Next)
            {
                if (!mapIdList.Contains(node.Value.MapId))
                    mapIdList.Add(node.Value.MapId);
                if (node.Value.IsStoppingPoint)
                    linkedListNodeList.Add(node);
            }

            this.m_stops = linkedListNodeList.ToArray();
        }

        private void AppendAccelerationNodes(LinkedListNode<PathVertex> stopVertexNode)
        {
        }

        private void InitializeConstants()
        {
            this.m_accelerationRate = (float) this.m_entry.AccelRate;
            this.m_moveSpeed = (float) this.m_entry.MoveSpeed;
            this.m_accelerationTime = this.m_moveSpeed / this.m_accelerationRate;
            this.m_accelerationDist = (float) ((double) this.m_accelerationRate * (double) this.m_accelerationTime *
                                               (double) this.m_accelerationTime * 0.5);
        }

        private float GetTimeByDistance(float distance)
        {
            if ((double) distance < (double) this.m_accelerationDist)
                return (float) Math.Sqrt(2.0 * (double) distance / (double) this.m_accelerationRate);
            return (distance - this.m_accelerationDist) / this.m_moveSpeed + this.m_accelerationTime;
        }

        private float GetDistanceByTime(float time)
        {
            if ((double) time <= (double) this.m_accelerationTime)
                return (float) ((double) this.m_accelerationRate * (double) time * (double) time * 0.5);
            return (time - this.m_accelerationTime) * this.m_moveSpeed + this.m_accelerationDist;
        }
    }
}