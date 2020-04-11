using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Util.Graphics;

namespace WCell.Core.Paths
{
    /// <summary>
    /// TODO: Recycle
    /// A nicely recyclable iterable Vector3 collection
    /// </summary>
    public class Path : IEnumerable<Vector3>, IEnumerable
    {
        private Vector3[] points;
        private int last;
        private int index;

        public Path()
        {
        }

        public Path(ICollection<Vector3> points)
        {
            this.Reset(points.Count);
            foreach (Vector3 point in (IEnumerable<Vector3>) points)
                this.Add(point);
        }

        /// <summary>
        /// Creates a new path that re-uses the given array of points.
        /// </summary>
        /// <param name="points"></param>
        public Path(Vector3[] points)
        {
            this.points = points;
            this.last = points.Length;
        }

        public Path(Path path)
        {
            this.points = path.points;
            this.index = path.index;
            this.last = path.last;
        }

        public Vector3 this[int i]
        {
            get { return this.points[i]; }
        }

        public Vector3 First
        {
            get { return this.points[0]; }
        }

        public Vector3 Next()
        {
            return this.points[this.index++];
        }

        public bool HasNext()
        {
            return this.index < this.points.Length;
        }

        public void Reset(int newSize)
        {
            if (this.points == null)
                this.points = new Vector3[newSize];
            else if (this.points.Length < newSize)
                Array.Resize<Vector3>(ref this.points, newSize);
            for (int index = 0; index < this.points.Length; ++index)
                this.points[index] = new Vector3(float.MinValue);
            this.last = this.points.Length;
        }

        /// <summary>
        /// Adds a new vector to the Path in reverse order
        /// i.e. the first added vector goes onto the end of the array, the next goes to (end - 1), etc.
        /// </summary>
        /// <param name="v"></param>
        public void Add(Vector3 v)
        {
            this.points[--this.last] = v;
            this.index = this.last;
        }

        /// <summary>
        /// Adds a list of new vectors to the Path in reverse order
        /// i.e. the first added vector goes onto the end of the array, the next goes onto (end - 1), etc.
        /// </summary>
        public void Add(IEnumerable<Vector3> list)
        {
            foreach (Vector3 vector3 in list)
            {
                this.points[--this.last] = vector3;
                this.index = this.last;
            }
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            for (int i = this.last; i < this.points.Length; ++i)
                yield return this.points[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }

        public Path Copy()
        {
            return new Path();
        }
    }
}