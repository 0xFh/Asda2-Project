using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Represents a division of map space (a node in any Map's quadtree).
    /// </summary>
    public class ZoneSpacePartitionNode
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The max depth of the QuadTrees used for space partitioning within Maps
        /// </summary>
        public static int DefaultPartitionThreshold = 6;

        public static float MinNodeLength = 250f;
        private const int Two = 2;
        private const int WEST = 0;
        private const int EAST = 1;
        private const int NORTH = 0;
        private const int SOUTH = 1;
        private const int HOR_EAST = 0;
        private const int HOR_CENTER = 1;
        private const int HOR_WEST = 2;
        private const int VER_NORTH = 0;
        private const int VER_CENTER = 1;
        private const int VER_SOUTH = 2;
        private BoundingBox m_bounds;
        private ZoneSpacePartitionNode[,] m_children;
        private Dictionary<EntityId, WorldObject> m_objects;

        /// <summary>
        /// Whether or not this node is a leaf node. (contains objects)
        /// </summary>
        public bool IsLeaf
        {
            get { return this.m_children == null; }
        }

        /// <summary>The dimensional bounds of this node.</summary>
        public BoundingBox Bounds
        {
            get { return this.m_bounds; }
        }

        /// <summary>The origin X of this node's bounds.</summary>
        public float X
        {
            get { return this.m_bounds.Min.X; }
        }

        /// <summary>The origin Y of this node's bounds.</summary>
        public float Y
        {
            get { return this.m_bounds.Min.Y; }
        }

        /// <summary>The length of this node.</summary>
        public float Length
        {
            get { return this.m_bounds.Max.X - this.m_bounds.Min.X; }
        }

        /// <summary>The width of this node.</summary>
        public float Width
        {
            get { return this.m_bounds.Max.Y - this.m_bounds.Min.Y; }
        }

        /// <summary>Whether or not this node has objects.</summary>
        public bool HasObjects
        {
            get
            {
                if (this.m_objects != null)
                    return this.m_objects.Count > 0;
                return false;
            }
        }

        /// <summary>Creates a node with the given bounds.</summary>
        /// <param name="bounds"></param>
        public ZoneSpacePartitionNode(BoundingBox bounds)
        {
            this.m_bounds = bounds;
        }

        /// <summary>
        /// Partitions the node, dividing it into subnodes until the desired depth is reached.
        /// </summary>
        /// <param name="maxLevels">the maximum depth to partition</param>
        /// <param name="startingDepth">the depth to start partitioning from</param>
        internal void PartitionSpace(ZoneSpacePartitionNode parent, int maxLevels, int startingDepth)
        {
            float num1 = this.Length / 2f;
            float num2 = this.Width / 2f;
            if (startingDepth < maxLevels && (double) num1 > (double) ZoneSpacePartitionNode.MinNodeLength &&
                (double) num2 > (double) ZoneSpacePartitionNode.MinNodeLength)
            {
                this.m_children = new ZoneSpacePartitionNode[2, 2];
                this.m_children[1, 0] = new ZoneSpacePartitionNode(new BoundingBox(
                    new Vector3(this.m_bounds.Min.X, this.m_bounds.Min.Y, this.m_bounds.Min.Z),
                    new Vector3(this.m_bounds.Min.X + num1, this.m_bounds.Min.Y + num2, this.m_bounds.Max.Z)));
                this.m_children[0, 0] = new ZoneSpacePartitionNode(new BoundingBox(
                    new Vector3(this.m_bounds.Min.X, this.m_bounds.Min.Y + num2, this.m_bounds.Min.Z),
                    new Vector3(this.m_bounds.Min.X + num1, this.m_bounds.Max.Y, this.m_bounds.Max.Z)));
                this.m_children[1, 1] = new ZoneSpacePartitionNode(new BoundingBox(
                    new Vector3(this.m_bounds.Min.X + num1, this.m_bounds.Min.Y, this.m_bounds.Min.Z),
                    new Vector3(this.m_bounds.Max.X, this.m_bounds.Min.Y + num2, this.m_bounds.Max.Z)));
                this.m_children[0, 1] = new ZoneSpacePartitionNode(new BoundingBox(
                    new Vector3(this.m_bounds.Min.X + num1, this.m_bounds.Min.Y + num2, this.m_bounds.Min.Z),
                    new Vector3(this.m_bounds.Max.X, this.m_bounds.Max.Y, this.m_bounds.Max.Z)));
                ++startingDepth;
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                        this.m_children[index1, index2].PartitionSpace(this, maxLevels, startingDepth);
                }
            }
            else
                this.m_objects = new Dictionary<EntityId, WorldObject>(0);
        }

        /// <summary>TODO: Find all intermediate neighbors</summary>
        /// <param name="parent"></param>
        /// <param name="vertical"></param>
        /// <param name="horizontal"></param>
        internal void FindNeighbors(ZoneSpacePartitionNode parent, int vertical, int horizontal)
        {
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <typeparam name="T">the specific type of the objects to retrieve</typeparam>
        /// <param name="entities">the list to append retrieved objects to</param>
        internal void GetObjectsOfSpecificType<T>(ref BoundingBox box, List<T> entities, uint phase,
            ref int limitCounter) where T : WorldObject
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (box.Contains(ref position) && worldObject.IsInPhase(phase) &&
                        worldObject.GetType() == typeof(T))
                    {
                        entities.Add(worldObject as T);
                        if (--limitCounter == 0)
                            break;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref box).HasAnyFlag(IntersectionType.Touches))
                            child.GetObjectsOfSpecificType<T>(ref box, entities, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <typeparam name="T">the minimum type of the objects to retrieve</typeparam>
        /// <param name="box">the area to search</param>
        /// <param name="entities">the list to append retrieved objects to</param>
        internal void GetEntitiesInArea<T>(ref BoundingBox box, List<T> entities, uint phase, ref int limitCounter)
            where T : WorldObject
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (box.Contains(ref position) && worldObject.IsInPhase(phase) && worldObject is T)
                    {
                        entities.Add(worldObject as T);
                        if (--limitCounter == 0)
                            break;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref box).HasAnyFlag(IntersectionType.Touches))
                            child.GetEntitiesInArea<T>(ref box, entities, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <typeparam name="T">the minimum type of the objects to retrieve</typeparam>
        /// <param name="box">the area to search</param>
        /// <param name="entities">the list to append retrieved objects to</param>
        internal void GetEntitiesInArea<T>(ref BoundingBox box, List<T> entities, Func<T, bool> filter, uint phase,
            ref int limitCounter) where T : WorldObject
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (box.Contains(ref position) && worldObject.IsInPhase(phase) && worldObject is T)
                    {
                        T obj = worldObject as T;
                        if (filter(obj))
                        {
                            entities.Add(obj);
                            if (--limitCounter == 0)
                                break;
                        }
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref box).HasAnyFlag(IntersectionType.Touches))
                            child.GetEntitiesInArea<T>(ref box, entities, filter, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <param name="box">the area to search</param>
        /// <param name="entities">the list to append retrieved objects to</param>
        /// <param name="filter">the type (in respect to the WoW client) that the object must be</param>
        internal void GetEntitiesInArea(ref BoundingBox box, List<WorldObject> entities, ObjectTypes filter, uint phase,
            ref int limitCounter)
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (box.Contains(ref position) && worldObject.IsInPhase(phase) &&
                        worldObject.Type.HasAnyFlag(filter))
                    {
                        entities.Add(worldObject);
                        if (--limitCounter == 0)
                            break;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref box).HasAnyFlag(IntersectionType.Touches))
                            child.GetEntitiesInArea(ref box, entities, filter, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <param name="box">the area to search</param>
        /// <param name="entities">the list to append retrieved objects to</param>
        /// <param name="filter">a predicate to determin whether or not to add specific objects</param>
        internal void GetEntitiesInArea(ref BoundingBox box, List<WorldObject> entities, Func<WorldObject, bool> filter,
            uint phase, ref int limitCounter)
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (box.Contains(ref position) && worldObject.IsInPhase(phase) && filter(worldObject))
                    {
                        entities.Add(worldObject);
                        if (--limitCounter == 0)
                            break;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref box).HasAnyFlag(IntersectionType.Touches))
                            child.GetEntitiesInArea(ref box, entities, filter, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <typeparam name="T">the specific type of the objects to retrieve</typeparam>
        /// <param name="entities">the list to append retrieved objects to</param>
        internal void GetObjectsOfSpecificType<T>(ref BoundingSphere sphere, List<T> entities, uint phase,
            ref int limitCounter) where T : WorldObject
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (sphere.Contains(ref position) && worldObject.IsInPhase(phase) &&
                        worldObject.GetType() == typeof(T))
                    {
                        entities.Add(worldObject as T);
                        if (--limitCounter == 0)
                            break;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref sphere).HasAnyFlag(IntersectionType.Touches))
                            child.GetObjectsOfSpecificType<T>(ref sphere, entities, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <typeparam name="T">the minimum type of the objects to retrieve</typeparam>
        /// <param name="sphere">the area to search</param>
        /// <param name="entities">the list to append retrieved objects to</param>
        internal void GetEntitiesInArea<T>(ref BoundingSphere sphere, List<T> entities, uint phase,
            ref int limitCounter) where T : WorldObject
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (sphere.Contains(ref position) && worldObject.IsInPhase(phase) && worldObject is T)
                    {
                        entities.Add(worldObject as T);
                        if (--limitCounter == 0)
                            break;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref sphere).HasAnyFlag(IntersectionType.Touches))
                            child.GetEntitiesInArea<T>(ref sphere, entities, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <typeparam name="T">the minimum type of the objects to retrieve</typeparam>
        /// <param name="sphere">the area to search</param>
        /// <param name="entities">the list to append retrieved objects to</param>
        internal void GetEntitiesInArea<T>(ref BoundingSphere sphere, List<T> entities, Func<T, bool> filter,
            uint phase, ref int limitCounter) where T : WorldObject
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (sphere.Contains(ref position) && worldObject.IsInPhase(phase) && worldObject is T)
                    {
                        T obj = worldObject as T;
                        if (filter(obj))
                        {
                            entities.Add(obj);
                            if (--limitCounter == 0)
                                break;
                        }
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref sphere).HasAnyFlag(IntersectionType.Touches))
                            child.GetEntitiesInArea<T>(ref sphere, entities, filter, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <param name="sphere">the area to search</param>
        /// <param name="entities">the list to append retrieved objects to</param>
        /// <param name="filter">the type (in respect to the WoW client) that the object must be</param>
        internal void GetEntitiesInArea(ref BoundingSphere sphere, List<WorldObject> entities, ObjectTypes filter,
            uint phase, ref int limitCounter)
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (sphere.Contains(ref position) && worldObject.IsInPhase(phase) &&
                        worldObject.Type.HasAnyFlag(filter))
                    {
                        entities.Add(worldObject);
                        if (--limitCounter == 0)
                            break;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref sphere).HasAnyFlag(IntersectionType.Touches))
                            child.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Gets all objects within a specified radius.</summary>
        /// <param name="entities">the list to append retrieved objects to</param>
        /// <param name="filter">a predicate to determin whether or not to add specific objects</param>
        internal void GetEntitiesInArea(ref BoundingSphere sphere, List<WorldObject> entities,
            Func<WorldObject, bool> filter, uint phase, ref int limitCounter)
        {
            if (this.IsLeaf)
            {
                if (!this.HasObjects)
                    return;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    Vector3 position = worldObject.Position;
                    if (sphere.Contains(ref position) && worldObject.IsInPhase(phase) && filter(worldObject))
                    {
                        entities.Add(worldObject);
                        if (--limitCounter == 0)
                            break;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref sphere).HasAnyFlag(IntersectionType.Touches))
                            child.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limitCounter);
                    }
                }
            }
        }

        /// <summary>Iterates over all objects in this Node.</summary>
        /// <param name="predicate">Returns whether to continue iteration.</param>
        /// <returns>Whether Iteration was not cancelled (usually indicating that we did not find what we were looking for).</returns>
        internal bool Iterate(ref BoundingSphere sphere, Func<WorldObject, bool> predicate, uint phase)
        {
            if (this.IsLeaf)
            {
                if (this.HasObjects)
                {
                    foreach (WorldObject worldObject in this.m_objects.Values)
                    {
                        Vector3 position = worldObject.Position;
                        if (sphere.Contains(ref position) && worldObject.IsInPhase(phase) && !predicate(worldObject))
                            return false;
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < 2; ++index2)
                    {
                        ZoneSpacePartitionNode child = this.m_children[index1, index2];
                        if (child.Bounds.Intersects(ref sphere).HasAnyFlag(IntersectionType.Touches) &&
                            !child.Iterate(ref sphere, predicate, phase))
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>Adds an object to the node.</summary>
        /// <param name="obj">the object to add</param>
        /// <returns>whether or not the object was added successfully</returns>
        internal bool AddObject(WorldObject obj)
        {
            if (this.IsLeaf)
            {
                if (this.m_objects.ContainsKey(obj.EntityId))
                    throw new ArgumentException(string.Format(
                        "Tried to add Object \"{0}\" with duplicate EntityId {1} to Map.", (object) obj,
                        (object) obj.EntityId));
                this.m_objects.Add(obj.EntityId, obj);
                obj.Node = this;
                return true;
            }

            Vector3 position = obj.Position;
            for (int index1 = 0; index1 < 2; ++index1)
            {
                for (int index2 = 0; index2 < 2; ++index2)
                {
                    ZoneSpacePartitionNode child = this.m_children[index1, index2];
                    if (child.Bounds.Contains(ref position))
                        return child.AddObject(obj);
                }
            }

            return false;
        }

        /// <summary>Gets a leaf node from a given point.</summary>
        /// <param name="pt">the point to retrieve the parent node from</param>
        /// <returns>the node which contains the given point; null if the point is invalid</returns>
        internal ZoneSpacePartitionNode GetLeafFromPoint(ref Vector3 pt)
        {
            if (this.IsLeaf)
            {
                if (this.m_bounds.Contains(ref pt))
                    return this;
                return (ZoneSpacePartitionNode) null;
            }

            for (int index1 = 0; index1 < 2; ++index1)
            {
                for (int index2 = 0; index2 < 2; ++index2)
                {
                    ZoneSpacePartitionNode child = this.m_children[index1, index2];
                    if (child.Bounds.Contains(ref pt))
                        return child.GetLeafFromPoint(ref pt);
                }
            }

            return (ZoneSpacePartitionNode) null;
        }

        /// <summary>Removes an object from the node.</summary>
        /// <param name="obj">the object to remove</param>
        /// <returns>whether or not the object was removed successfully</returns>
        internal bool RemoveObject(WorldObject obj)
        {
            if (this.IsLeaf)
                return this.m_objects.Remove(obj.EntityId);
            Vector3 position = obj.Position;
            for (int index1 = 0; index1 < 2; ++index1)
            {
                for (int index2 = 0; index2 < 2; ++index2)
                {
                    ZoneSpacePartitionNode child = this.m_children[index1, index2];
                    if (child.Bounds.Contains(ref position))
                        return child.RemoveObject(obj);
                }
            }

            return false;
        }

        public override string ToString()
        {
            return this.GetType().Name +
                   (!this.IsLeaf ? (object) "" : (object) (" (" + (object) this.m_objects.Count + " Objects)")) +
                   (object) this.m_bounds;
        }
    }
}