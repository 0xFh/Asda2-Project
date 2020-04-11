using System.Collections.Generic;

namespace WCell.Constants.World
{
  public struct SimpleZoneGrid : IZoneGrid
  {
    public readonly uint Id;

    public SimpleZoneGrid(uint id)
    {
      Id = id;
    }

    public ZoneId GetZoneId(int x, int y)
    {
      return (ZoneId) Id;
    }

    public IEnumerable<ZoneId> GetAllZoneIds()
    {
      yield return (ZoneId) Id;
    }
  }
}