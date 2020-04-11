namespace WCell.Constants.World
{
  public class ZoneTileSet
  {
    public readonly IZoneGrid[,] ZoneGrids;

    public ZoneTileSet()
    {
      ZoneGrids = new IZoneGrid[64, 64];
    }

    public ZoneTileSet(IZoneGrid[,] grids)
    {
      ZoneGrids = grids;
    }

    public ZoneId GetZoneId(float x, float y)
    {
      x = (float) ((17066.666015625 - x) / 533.333312988281);
      y = (float) ((17066.666015625 - y) / 533.333312988281);
      int index1 = (int) x;
      int index2 = (int) y;
      if(index1 >= 64 || index2 >= 64 || index1 < 0 || index2 < 0)
        return ZoneId.None;
      IZoneGrid zoneGrid = ZoneGrids[index2, index1];
      if(zoneGrid == null)
        return ZoneId.None;
      x = (float) ((x - (double) index1) * 16.0);
      y = (float) ((y - (double) index2) * 16.0);
      int y1 = (int) x;
      int x1 = (int) y;
      if(y1 >= 16 || x1 >= 16 || y1 < 0 || x1 < 0)
        return ZoneId.None;
      return zoneGrid.GetZoneId(x1, y1);
    }
  }
}