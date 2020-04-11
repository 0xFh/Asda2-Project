using WCell.Util.Graphics;

namespace WCell.Constants
{
    public static class TerrainConstants
    {
        /// <summary>
        /// The width/height of 1 of the 64x64 tiles that compose a full map
        /// </summary>
        public const float TileSize = 533.3333f;

        /// <summary>
        /// The width/height of 1 of the 16x16 chunks that compose a tile
        /// </summary>
        public const float ChunkSize = 33.33333f;

        /// <summary>
        /// The width/height of 1 of the 8x8 units that compose a chunk
        /// </summary>
        public const float UnitSize = 4.166667f;

        public const int TilesPerMapSide = 64;
        public const int ChunksPerTileSide = 16;
        public const int ChunksPerTile = 256;
        public const int UnitsPerChunkSide = 8;

        /// <summary>The Center of a full 64x64 map</summary>
        public const float CenterPoint = 17066.67f;

        /// <summary>The highest possible Z component on a Map</summary>
        public const float MaxHeight = 2048f;

        /// <summary>The lowest possible Z component on a Map</summary>
        public const float MinHeight = 2048f;

        /// <summary>The length of a side of the 64x64 map</summary>
        public const float MapLength = 34133.33f;

        public const float TerrainSimplificationConst = 0.005f;
        public const float H2OSimplificationConst = 0.005f;
        public const float WMOSimplificationConst = 0.0f;
        public const float M2SimplificationConst = 0.0f;

        /// <summary>The lowest X/Y value possible</summary>
        public const float MinPlain = -17066.67f;

        /// <summary>The highest X/Y value possible</summary>
        public const float MaxPlain = 17066.67f;

        public const string MapFileExtension = "map";
        public const string TileFilenameFormat = "{0:00}_{1:00}";
        public const string WMOFileExtension = "wmo";
        public const string WMOFileFormat = "{0:00}_{1:00}.wmo";
        public const string M2FileExtension = "m2x";
        public const string M2FileFormat = "{0:00}_{1:00}.m2x";

        public static string GetTileName(int tileX, int tileY)
        {
            return string.Format("{0:00}_{1:00}", (object) tileX, (object) tileY);
        }

        public static string GetMapFilename(int tileX, int tileY)
        {
            return TerrainConstants.GetTileName(tileY, tileX) + "map";
        }

        public static string GetWMOFile(int x, int y)
        {
            return string.Format("{0:00}_{1:00}.wmo", (object) x, (object) y);
        }

        public static string GetM2File(int x, int y)
        {
            return string.Format("{0:00}_{1:00}.m2x", (object) x, (object) y);
        }

        public static string GetADTFileName(string wdtName, int tileX, int tileY)
        {
            return string.Format("{0}_{1:00}_{2:00}", (object) wdtName, (object) tileY, (object) tileX);
        }

        public static void TilePositionToWorldPosition(ref Vector3 tilePosition)
        {
            tilePosition.X = (float) (((double) tilePosition.X - 17066.666015625) * -1.0);
            tilePosition.Y = (float) (((double) tilePosition.Y - 17066.666015625) * -1.0);
        }

        public static void TileExtentsToWorldExtents(ref BoundingBox tileExtents)
        {
            TerrainConstants.TilePositionToWorldPosition(ref tileExtents.Min);
            TerrainConstants.TilePositionToWorldPosition(ref tileExtents.Max);
        }
    }
}