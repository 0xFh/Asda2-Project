using System.Collections.Generic;

namespace WCell.Constants.World
{
    public struct ZoneGrid : IZoneGrid
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly uint[,] ZoneIds;

        public ZoneGrid(uint[,] ids)
        {
            this.ZoneIds = ids;
        }

        public ZoneId GetZoneId(int col, int row)
        {
            return (ZoneId) this.ZoneIds[col, row];
        }

        public IEnumerable<ZoneId> GetAllZoneIds()
        {
            HashSet<ZoneId> set = new HashSet<ZoneId>();
            uint[,] zoneIds = this.ZoneIds;
            int upperBound1 = zoneIds.GetUpperBound(0);
            int upperBound2 = zoneIds.GetUpperBound(1);
            for (int lowerBound1 = zoneIds.GetLowerBound(0); lowerBound1 <= upperBound1; ++lowerBound1)
            {
                for (int lowerBound2 = zoneIds.GetLowerBound(1); lowerBound2 <= upperBound2; ++lowerBound2)
                {
                    ZoneId id = (ZoneId) zoneIds[lowerBound1, lowerBound2];
                    if (id != ZoneId.None && !set.Contains(id))
                    {
                        set.Add(id);
                        yield return id;
                    }
                }
            }
        }
    }
}