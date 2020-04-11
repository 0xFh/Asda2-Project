using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
  public class SimpleWorldLocation : IWorldLocation, IHasPosition
  {
    public SimpleWorldLocation(MapId map, Vector3 pos, uint phase = 1)
    {
      Position = pos;
      MapId = map;
      Phase = phase;
    }

    public Vector3 Position { get; set; }

    public MapId MapId { get; set; }

    public Map Map
    {
      get { return World.GetNonInstancedMap(MapId); }
    }

    public uint Phase { get; set; }
  }
}