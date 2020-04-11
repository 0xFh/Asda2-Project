using System.Collections.Generic;

namespace WCell.Constants.World
{
    public interface IZoneGrid
    {
        ZoneId GetZoneId(int x, int y);

        IEnumerable<ZoneId> GetAllZoneIds();
    }
}