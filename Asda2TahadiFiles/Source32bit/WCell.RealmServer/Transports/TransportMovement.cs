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
      m_entry = entry;
      Initialize();
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
      InitializeConstants();
      FindStopsAndMapIds(m_entry.Path.Nodes.First);
      if(m_stops.Length != 2)
        return;
      LinkedListNode<PathVertex> stop1 = m_stops[0];
      LinkedListNode<PathVertex> stop2 = m_stops[1];
      AppendAccelerationNodes(stop1);
    }

    private void FindStopsAndMapIds(LinkedListNode<PathVertex> node)
    {
      List<MapId> mapIdList = new List<MapId>();
      List<LinkedListNode<PathVertex>> linkedListNodeList = new List<LinkedListNode<PathVertex>>();
      for(; node != null; node = node.Next)
      {
        if(!mapIdList.Contains(node.Value.MapId))
          mapIdList.Add(node.Value.MapId);
        if(node.Value.IsStoppingPoint)
          linkedListNodeList.Add(node);
      }

      m_stops = linkedListNodeList.ToArray();
    }

    private void AppendAccelerationNodes(LinkedListNode<PathVertex> stopVertexNode)
    {
    }

    private void InitializeConstants()
    {
      m_accelerationRate = m_entry.AccelRate;
      m_moveSpeed = m_entry.MoveSpeed;
      m_accelerationTime = m_moveSpeed / m_accelerationRate;
      m_accelerationDist = (float) (m_accelerationRate * (double) m_accelerationTime *
                                    m_accelerationTime * 0.5);
    }

    private float GetTimeByDistance(float distance)
    {
      if(distance < (double) m_accelerationDist)
        return (float) Math.Sqrt(2.0 * distance / m_accelerationRate);
      return (distance - m_accelerationDist) / m_moveSpeed + m_accelerationTime;
    }

    private float GetDistanceByTime(float time)
    {
      if(time <= (double) m_accelerationTime)
        return (float) (m_accelerationRate * (double) time * time * 0.5);
      return (time - m_accelerationTime) * m_moveSpeed + m_accelerationDist;
    }
  }
}