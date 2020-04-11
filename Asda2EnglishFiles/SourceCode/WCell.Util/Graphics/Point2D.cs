using System;

namespace WCell.Util.Graphics
{
    /// <summary>Fixed 2D point (using integers for components)</summary>
    public class Point2D : IEquatable<Point2D>
    {
        public static Point2D NorthWest = new Point2D(-1, -1);
        public static Point2D North = new Point2D(-1, 0);
        public static Point2D NorthEast = new Point2D(-1, 1);
        public static Point2D East = new Point2D(0, 1);
        public static Point2D SouthEast = new Point2D(1, 1);
        public static Point2D South = new Point2D(1, 0);
        public static Point2D SouthWest = new Point2D(1, -1);
        public static Point2D West = new Point2D(0, -1);
        public int X;
        public int Y;

        public Point2D()
        {
        }

        public Point2D(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals((object) null, obj))
                return false;
            if (object.ReferenceEquals((object) this, obj))
                return true;
            if (obj.GetType() != typeof(Point2D))
                return false;
            return this.Equals((Point2D) obj);
        }

        public bool Equals(Point2D obj)
        {
            if (object.ReferenceEquals((object) null, (object) obj))
                return false;
            if (object.ReferenceEquals((object) this, (object) obj))
                return true;
            return obj.X == this.X && obj.Y == this.Y;
        }

        public override int GetHashCode()
        {
            return this.X * 397 ^ this.Y;
        }

        public static bool operator ==(Point2D left, Point2D right)
        {
            return object.Equals((object) left, (object) right);
        }

        public static bool operator !=(Point2D left, Point2D right)
        {
            return !object.Equals((object) left, (object) right);
        }

        public static Point2D operator +(Point2D left, Point2D right)
        {
            return new Point2D(left.X + right.X, left.Y + right.Y);
        }

        public static Point2D operator -(Point2D left, Point2D right)
        {
            return new Point2D(left.X - right.X, left.Y - right.Y);
        }
    }
}