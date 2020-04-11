using System;
using WCell.Constants.World;
using WCell.Core.Terrain;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Waypoints
{
    public class RandomWaypointGenerator : WaypointGenerator
    {
        public static Vector3[] GenerateWaypoints(MapId map, Vector3 lastPos)
        {
            ITerrain terrain = TerrainMgr.GetTerrain(map);
            if (terrain != null)
                return new RandomWaypointGenerator().GenerateWaypoints(terrain, lastPos);
            return new Vector3[0];
        }

        public Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos)
        {
            return this.GenerateWaypoints(terrain, lastPos, 5, 10, 5f, 10f);
        }

        public Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos, float radius)
        {
            int count = Math.Max(3, (int) ((double) Math.Min(1f, radius * radius) / 5.0) * 2);
            return this.GenerateWaypoints(terrain, lastPos, radius, count);
        }

        public Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos, int min, int max, float minDist,
            float maxDist)
        {
            if (min < 1)
                throw new ArgumentException("The minimum point count must be greater than 1", nameof(min));
            if (max < min)
                throw new ArgumentException("The maximum point count must be greater than the minimum", nameof(max));
            int count = Utility.Random(min, max);
            return RandomWaypointGenerator.GenerateWaypoints(terrain, lastPos, minDist, maxDist, count);
        }

        public Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 centrePos, float radius, int count)
        {
            Vector3[] vector3Array = new Vector3[count];
            for (int index = 0; index < count; ++index)
            {
                float num1 = Utility.RandomFloat() * 6.283185f;
                float num2 = (float) Math.Sqrt((double) Utility.Random(0.0f, radius));
                Vector3 worldPos = new Vector3();
                worldPos.X = centrePos.X + radius * num2 * (float) Math.Cos((double) num1);
                worldPos.Y = centrePos.Y + radius * num2 * (float) Math.Sin((double) num1);
                worldPos.Z = centrePos.Z;
                worldPos.Z = terrain.GetGroundHeightUnderneath(worldPos);
                vector3Array[index] = worldPos;
            }

            return vector3Array;
        }

        public static Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos, float minDist, float maxDist,
            int count)
        {
            if ((double) maxDist < (double) minDist)
                throw new ArgumentException("The maximum waypoint distance must be greater than the minimum",
                    nameof(maxDist));
            Vector3[] vector3Array = new Vector3[count];
            for (int index = 0; index < count; ++index)
            {
                float angle = Utility.Random(0.0f, 6.283185f);
                float dist = Utility.Random(minDist, maxDist);
                float z = lastPos.Z;
                lastPos.GetPointYX(angle, dist, out lastPos);
                lastPos.Z = z;
                lastPos.Z = terrain.GetGroundHeightUnderneath(lastPos);
                vector3Array[index] = lastPos;
            }

            return vector3Array;
        }
    }
}