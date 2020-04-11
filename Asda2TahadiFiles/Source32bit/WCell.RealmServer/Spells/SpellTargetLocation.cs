using System;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
  [Serializable]
  public class SpellTargetLocation : IWorldLocation, IHasPosition
  {
    private Vector3 m_Position;

    public SpellTargetLocation()
    {
      Phase = 1U;
    }

    public SpellTargetLocation(MapId map, Vector3 pos, uint phase = 1)
    {
      Position = pos;
      MapId = map;
      Phase = phase;
    }

    public Vector3 Position
    {
      get { return m_Position; }
      set { m_Position = value; }
    }

    [Persistent]
    public MapId MapId { get; set; }

    [Persistent]
    public float X
    {
      get { return m_Position.X; }
      set { m_Position.X = value; }
    }

    [Persistent]
    public float Y
    {
      get { return m_Position.Y; }
      set { m_Position.Y = value; }
    }

    [Persistent]
    public float Z
    {
      get { return m_Position.Z; }
      set { m_Position.Z = value; }
    }

    public Map Map
    {
      get { return World.GetNonInstancedMap(MapId); }
    }

    public uint Phase { get; set; }
  }
}