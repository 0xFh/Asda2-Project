using System.IO;
using WCell.Util.Graphics;

namespace WCell.Constants
{
    public static class PositionUtil
    {
        /// <summary>
        /// Calculates which Tile the given position belongs to on a Map.
        /// </summary>
        /// <param name="worldPos">Calculate the Tile coords for this position.</param>
        /// <param name="tileX">Set to the X coordinate of the tile.</param>
        /// <param name="tileY">Set to the Y coordinate of the tile.</param>
        /// <returns>True if the tile (X, Y) is valid.</returns>
        public static bool GetTileXYForPos(Vector3 worldPos, out int tileX, out int tileY)
        {
            tileX = (int) PositionUtil.GetTileFraction(worldPos.X);
            tileY = (int) PositionUtil.GetTileFraction(worldPos.Y);
            return PositionUtil.VerifyTileCoords(tileX, tileY);
        }

        public static void GetChunkXYForPos(Vector3 worldPos, out int chunkX, out int chunkY)
        {
            float tileFraction1 = PositionUtil.GetTileFraction(worldPos.X);
            float tileFraction2 = PositionUtil.GetTileFraction(worldPos.Y);
            chunkX = (int) PositionUtil.GetChunkFraction(tileFraction1);
            chunkY = (int) PositionUtil.GetChunkFraction(tileFraction2);
        }

        public static Rect GetTileBoundingRect(int tileX, int tileY)
        {
            float x = (float) (17066.666015625 - (double) tileX * 533.333312988281);
            float num1 = x - 533.3333f;
            float y = (float) (17066.666015625 - (double) tileY * 533.333312988281);
            float num2 = y - 533.3333f;
            return new Rect(new Point(x, y), new Point(533.3333f, 533.3333f));
        }

        private static float GetTileFraction(float loc)
        {
            return (float) ((17066.666015625 - (double) loc) / 533.333312988281);
        }

        private static float GetChunkFraction(float tileFraction)
        {
            return (float) (((double) tileFraction - (double) (int) tileFraction) * 16.0);
        }

        public static bool VerifyTileCoords(int tileX, int tileY)
        {
            bool flag = true;
            if (tileX < 0)
                flag = false;
            if (tileX >= 64)
                flag = false;
            if (tileY < 0)
                flag = false;
            if (tileX >= 64)
                flag = false;
            return flag;
        }

        public static Point2D GetTileXYForPos(Vector3 worldPos)
        {
            int tileFraction1 = (int) PositionUtil.GetTileFraction(worldPos.X);
            int tileFraction2 = (int) PositionUtil.GetTileFraction(worldPos.Y);
            PositionUtil.VerifyTileCoords(tileFraction1, tileFraction2);
            return new Point2D()
            {
                X = tileFraction1,
                Y = tileFraction2
            };
        }

        public static Point2D GetXYForPos(Vector3 worldPos, out Point2D tileCoord)
        {
            float tileFraction1 = PositionUtil.GetTileFraction(worldPos.X);
            float tileFraction2 = PositionUtil.GetTileFraction(worldPos.Y);
            int tileX = (int) tileFraction1;
            int tileY = (int) tileFraction2;
            PositionUtil.VerifyTileCoords(tileX, tileY);
            tileCoord = new Point2D() {X = tileX, Y = tileY};
            int chunkFraction1 = (int) PositionUtil.GetChunkFraction(tileFraction1);
            int chunkFraction2 = (int) PositionUtil.GetChunkFraction(tileFraction2);
            PositionUtil.VerifyPoint2D(worldPos, (int) tileFraction1, (int) tileFraction2, chunkFraction1,
                chunkFraction2);
            return new Point2D()
            {
                X = chunkFraction1,
                Y = chunkFraction2
            };
        }

        public static Point2D GetHeightMapXYForPos(Vector3 worldPos, out Point2D tileCoord, out Point2D chunkCoord)
        {
            float tileFraction1 = PositionUtil.GetTileFraction(worldPos.X);
            float tileFraction2 = PositionUtil.GetTileFraction(worldPos.Y);
            int tileX = (int) tileFraction1;
            int tileY = (int) tileFraction2;
            PositionUtil.VerifyTileCoords(tileX, tileY);
            tileCoord = new Point2D() {X = tileX, Y = tileY};
            float chunkFraction1 = PositionUtil.GetChunkFraction(tileFraction1);
            float chunkFraction2 = PositionUtil.GetChunkFraction(tileFraction2);
            int chunkX = (int) chunkFraction1;
            int chunkY = (int) chunkFraction2;
            PositionUtil.VerifyPoint2D(worldPos, tileX, tileY, chunkX, chunkY);
            chunkCoord = new Point2D() {X = chunkX, Y = chunkY};
            int heightMapFraction1 = (int) PositionUtil.GetHeightMapFraction(chunkFraction1);
            int heightMapFraction2 = (int) PositionUtil.GetHeightMapFraction(chunkFraction2);
            return new Point2D()
            {
                X = heightMapFraction1,
                Y = heightMapFraction2
            };
        }

        public static HeightMapFraction GetHeightMapFraction(Vector3 worldPos, out Point2D tileCoord,
            out Point2D chunkCoord, out Point2D unitCoord)
        {
            float tileFraction1 = PositionUtil.GetTileFraction(worldPos.X);
            float tileFraction2 = PositionUtil.GetTileFraction(worldPos.Y);
            int tileX = (int) tileFraction1;
            int tileY = (int) tileFraction2;
            PositionUtil.VerifyTileCoords(tileX, tileY);
            tileCoord = new Point2D() {X = tileX, Y = tileY};
            float chunkFraction1 = PositionUtil.GetChunkFraction(tileFraction1);
            float chunkFraction2 = PositionUtil.GetChunkFraction(tileFraction2);
            int chunkX = (int) chunkFraction1;
            int chunkY = (int) chunkFraction2;
            PositionUtil.VerifyPoint2D(worldPos, tileX, tileY, chunkX, chunkY);
            chunkCoord = new Point2D() {X = chunkX, Y = chunkY};
            float heightMapFraction1 = PositionUtil.GetHeightMapFraction(chunkFraction1);
            float heightMapFraction2 = PositionUtil.GetHeightMapFraction(chunkFraction2);
            int heightMapX = (int) heightMapFraction1;
            int heightMapY = (int) heightMapFraction2;
            PositionUtil.VerifyHeightMapCoord(worldPos, tileX, tileY, chunkX, chunkY, heightMapX, heightMapY);
            unitCoord = new Point2D()
            {
                X = heightMapX,
                Y = heightMapY
            };
            return new HeightMapFraction()
            {
                FractionX = heightMapFraction1 - (float) heightMapX,
                FractionY = heightMapFraction2 - (float) heightMapY
            };
        }

        private static float GetHeightMapFraction(float chunkLocFraction)
        {
            return (float) (((double) chunkLocFraction - (double) (int) chunkLocFraction) * 8.0);
        }

        public static void VerifyPoint2D(Vector3 worldPos, int tileX, int tileY, int chunkX, int chunkY)
        {
            if (chunkX < 0)
                throw new InvalidDataException(string.Format(
                    "WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}]. chunkX < 0.",
                    (object) worldPos, (object) tileX, (object) tileY));
            if (chunkX >= 16)
                throw new InvalidDataException(string.Format(
                    "WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}]. chunkX >= {3}.",
                    (object) worldPos, (object) tileX, (object) tileY, (object) 16));
            if (chunkY < 0)
                throw new InvalidDataException(string.Format(
                    "WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}]. chunkY < 0.",
                    (object) worldPos, (object) tileX, (object) tileY));
            if (chunkY >= 16)
                throw new InvalidDataException(string.Format(
                    "WorldPos: {0} does not correspond to a valid chunk in Tile:[{1}, {2}]. chunkY >= {3}.",
                    (object) worldPos, (object) tileX, (object) tileY, (object) 16));
        }

        public static bool VerifyHeightMapCoord(Vector3 worldPos, int tileX, int tileY, int chunkX, int chunkY,
            int heightMapX, int heightMapY)
        {
            return heightMapX >= 0 && heightMapX <= 8 && (heightMapY >= 0 && heightMapY <= 8);
        }
    }
}