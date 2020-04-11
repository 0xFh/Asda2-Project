namespace WCell.Util.Graphics
{
    public struct Point
    {
        public static readonly Point Empty = new Point(0.0f, 0.0f);
        private float x;
        private float y;

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        public float Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        public bool IsEmpty
        {
            get { return (double) this.x == 0.0 && (double) this.y == 0.0; }
        }

        public static Point Add(Point pt, Size sz)
        {
            return new Point(pt.X + sz.Width, pt.Y + sz.Height);
        }

        public static Point operator +(Point pt, Size sz)
        {
            return Point.Add(pt, sz);
        }

        public static Point Subtract(Point pt, Size sz)
        {
            return new Point(pt.X - sz.Width, pt.Y - sz.Height);
        }

        public static Point operator -(Point pt, Size sz)
        {
            return Point.Subtract(pt, sz);
        }

        public Size ToSize()
        {
            return new Size(this.X, this.Y);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals((object) null, obj))
                return false;
            if (object.ReferenceEquals((object) this, obj))
                return true;
            if (obj.GetType() != typeof(Point))
                return false;
            return this.Equals((Point) obj);
        }

        public bool Equals(Point other)
        {
            if (object.ReferenceEquals((object) null, (object) other))
                return false;
            if (object.ReferenceEquals((object) this, (object) other))
                return true;
            return other.x.Equals(this.x) && other.y.Equals(this.y);
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() * 397 ^ this.y.GetHashCode();
        }

        public static bool operator ==(Point left, Point right)
        {
            return object.Equals((object) left, (object) right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !object.Equals((object) left, (object) right);
        }

        public override string ToString()
        {
            return string.Format("X = {0}, Y = {1}", (object) this.X, (object) this.Y);
        }
    }
}