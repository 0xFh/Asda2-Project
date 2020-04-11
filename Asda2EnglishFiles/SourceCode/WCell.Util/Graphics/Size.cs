namespace WCell.Util.Graphics
{
    public struct Size
    {
        public static readonly Size Empty = new Size(0.0f, 0.0f);
        public float width;
        public float height;

        public Size(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public float Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public float Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public bool IsEmpty
        {
            get { return (double) this.Width == 0.0 && (double) this.Height == 0.0; }
        }

        public static Size Add(Size sz1, Size sz2)
        {
            return new Size(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
        }

        public static Size operator +(Size sz1, Size sz2)
        {
            return Size.Add(sz1, sz2);
        }

        public static Size Subtract(Size sz1, Size sz2)
        {
            return new Size(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
        }

        public static Size operator -(Size sz1, Size sz2)
        {
            return Size.Subtract(sz1, sz2);
        }

        public Point ToPoint()
        {
            return new Point(this.width, this.height);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals((object) null, obj))
                return false;
            if (object.ReferenceEquals((object) this, obj))
                return true;
            if (obj.GetType() != typeof(Size))
                return false;
            return this.Equals((Size) obj);
        }

        public bool Equals(Size other)
        {
            if (object.ReferenceEquals((object) null, (object) other))
                return false;
            if (object.ReferenceEquals((object) this, (object) other))
                return true;
            return other.width.Equals(this.width) && other.height.Equals(this.height);
        }

        public override int GetHashCode()
        {
            return this.width.GetHashCode() * 397 ^ this.height.GetHashCode();
        }

        public static bool operator ==(Size left, Size right)
        {
            return object.Equals((object) left, (object) right);
        }

        public static bool operator !=(Size left, Size right)
        {
            return !object.Equals((object) left, (object) right);
        }
    }
}