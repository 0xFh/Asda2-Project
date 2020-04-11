using System;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
  public struct WorldLocationStruct : IWorldLocation, IHasPosition
  {
    private Vector3 m_Position;
    private Map m_Map;
    private uint m_Phase;

    public WorldLocationStruct(MapId map, Vector3 pos, uint phase = 1)
    {
      m_Position = pos;
      m_Map = World.GetNonInstancedMap(map);
      if(m_Map == null)
        throw new Exception("Invalid Map in WorldLocationStruct: " + map);
      m_Phase = phase;
    }

    public WorldLocationStruct(Map map, Vector3 pos, uint phase = 1)
    {
      m_Position = pos;
      m_Map = map;
      m_Phase = phase;
    }

    public Vector3 Position
    {
      get { return m_Position; }
      set { m_Position = value; }
    }

    public Map Map
    {
      get { return m_Map; }
      set { m_Map = value; }
    }

    public uint Phase
    {
      get { return m_Phase; }
      set { m_Phase = value; }
    }

    public MapId MapId
    {
      get { return Map.Id; }
    }
  }
}