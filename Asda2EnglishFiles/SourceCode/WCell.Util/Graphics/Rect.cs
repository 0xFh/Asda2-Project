using System;

namespace WCell.Util.Graphics
{
    /// <summary>
    /// Represents a Rectangle in WoW coordinate space
    /// i.e. X-positive into the screen, Y-Positive to the left
    /// </summary>
    public struct Rect
    {
        private static readonly Rect s_empty = new Rect()
        {
            x = float.PositiveInfinity,
            y = float.PositiveInfinity,
            width = float.NegativeInfinity,
            height = float.NegativeInfinity
        };

        internal float x;
        internal float y;
        internal float width;
        internal float height;

        public static bool operator ==(Rect rect1, Rect rect2)
        {
            return (double) rect1.X == (double) rect2.X && (double) rect1.Y == (double) rect2.Y &&
                   (double) rect1.Width == (double) rect2.Width && (double) rect1.Height == (double) rect2.Height;
        }

        public static bool operator !=(Rect rect1, Rect rect2)
        {
            return !(rect1 == rect2);
        }

        public static bool Equals(Rect rect1, Rect rect2)
        {
            if (rect1.IsEmpty)
                return rect2.IsEmpty;
            return rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Width.Equals(rect2.Width) &&
                   rect1.Height.Equals(rect2.Height);
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Rect))
                return false;
            return Rect.Equals(this, (Rect) o);
        }

        public bool Equals(Rect value)
        {
            return Rect.Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (this.IsEmpty)
                return 0;
            int hashCode1 = this.X.GetHashCode();
            float num1 = this.Y;
            int hashCode2 = num1.GetHashCode();
            int num2 = hashCode1 ^ hashCode2;
            num1 = this.Width;
            int hashCode3 = num1.GetHashCode();
            int num3 = num2 ^ hashCode3;
            num1 = this.Height;
            int hashCode4 = num1.GetHashCode();
            return num3 ^ hashCode4;
        }

        public override string ToString()
        {
            if (this.IsEmpty)
                return "Empty";
            return string.Format("X: {0}, Y: {1}, Width: {2}, Height: {3}", (object) this.x, (object) this.y,
                (object) this.width, (object) this.height);
        }

        public Rect(Point location, Size size)
        {
            if (size.IsEmpty)
            {
                this = Rect.s_empty;
            }
            else
            {
                this.x = location.X;
                this.y = location.Y;
                this.width = size.Width;
                this.height = size.Height;
            }
        }

        public Rect(float x, float y, float width, float height)
        {
            if ((double) width < 0.0 || (double) height < 0.0)
                throw new ArgumentException("Width and Height cannot be negative");
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rect(Point point1, Point point2)
        {
            this.x = Math.Min(point1.X, point2.X);
            this.y = Math.Min(point1.Y, point2.Y);
            this.width = Math.Max(Math.Max(point1.X, point2.X) - this.x, 0.0f);
            this.height = Math.Max(Math.Max(point1.Y, point2.Y) - this.y, 0.0f);
        }

        public Rect(Size size)
        {
            if (size.IsEmpty)
            {
                this = Rect.s_empty;
            }
            else
            {
                this.x = this.y = 0.0f;
                this.width = size.Width;
                this.height = size.Height;
            }
        }

        public static Rect Empty
        {
            get { return Rect.s_empty; }
        }

        public bool IsEmpty
        {
            get { return (double) this.width < 0.0; }
        }

        public Point Location
        {
            get { return new Point(this.x, this.y); }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                this.x = value.X;
                this.y = value.Y;
            }
        }

        public Size Size
        {
            get
            {
                if (this.IsEmpty)
                    return Size.Empty;
                return new Size(this.width, this.height);
            }
            set
            {
                if (value.IsEmpty)
                {
                    this = Rect.s_empty;
                }
                else
                {
                    if (this.IsEmpty)
                        throw new InvalidOperationException("Cannot modify empty Rect.");
                    this.width = value.Width;
                    this.height = value.Height;
                }
            }
        }

        public float X
        {
            get { return this.x; }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                this.x = value;
            }
        }

        public float Y
        {
            get { return this.y; }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                this.y = value;
            }
        }

        public float Width
        {
            get { return this.width; }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                if ((double) value < 0.0)
                    throw new ArgumentException("Width cannot be negative.");
                this.width = value;
            }
        }

        public float Height
        {
            get { return this.height; }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                if ((double) value < 0.0)
                    throw new ArgumentException("Height cannot be negative");
                this.height = value;
            }
        }

        public float Left
        {
            get
            {
                if (this.IsEmpty)
                    return float.NegativeInfinity;
                return this.y + this.width;
            }
        }

        public float Top
        {
            get
            {
                if (this.IsEmpty)
                    return float.NegativeInfinity;
                return this.x + this.height;
            }
        }

        public float Right
        {
            get { return this.y; }
        }

        public float Bottom
        {
            get { return this.x; }
        }

        public Point TopLeft
        {
            get { return new Point(this.Top, this.Left); }
        }

        public Point TopRight
        {
            get { return new Point(this.Top, this.Right); }
        }

        public Point BottomLeft
        {
            get { return new Point(this.Bottom, this.Left); }
        }

        public Point BottomRight
        {
            get { return new Point(this.Bottom, this.Right); }
        }

        /// <summary>
        /// Whether the rectangle contains the given Point.
        /// Points laying on the rectangles border are considered to be contained.
        /// </summary>
        public bool Contains(Point point)
        {
            return this.Contains(point.X, point.Y);
        }

        /// <summary>
        /// Whether the rectangle contains the given Point(x, y).
        /// Points laying on the rectangles border are considered to be contained.
        /// </summary>
        public bool Contains(float xPos, float yPos)
        {
            if (this.IsEmpty)
                return false;
            return this.ContainsInternal(xPos, yPos);
        }

        public bool Contains(Rect rect)
        {
            if (this.IsEmpty || rect.IsEmpty)
                return false;
            return (double) this.x <= (double) rect.x && (double) this.y <= (double) rect.y &&
                   (double) this.x + (double) this.height >= (double) rect.x + (double) rect.height &&
                   (double) this.y + (double) this.width >= (double) rect.y + (double) rect.width;
        }

        public bool IntersectsWith(Rect rect)
        {
            if (this.IsEmpty || rect.IsEmpty)
                return false;
            return (double) rect.Left <= (double) this.Right && (double) rect.Right >= (double) this.Left &&
                   (double) rect.Top <= (double) this.Bottom && (double) rect.Bottom >= (double) this.Top;
        }

        public bool IntersectsWith(Ray2D ray)
        {
            return this.IntersectWith(ray).HasValue;
        }

        public float? IntersectWith(Ray2D ray)
        {
            float f2_1 = 0.0f;
            float f2_2 = float.MaxValue;
            if ((double) Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.X < (double) this.X || (double) ray.Position.X > (double) this.Right)
                    return new float?();
            }
            else
            {
                float num1 = 1f / ray.Direction.X;
                float f1_1 = (this.X - ray.Position.X) * num1;
                float f1_2 = (this.Right - ray.Position.X) * num1;
                if ((double) f1_1 > (double) f1_2)
                {
                    float num2 = f1_1;
                    f1_1 = f1_2;
                    f1_2 = num2;
                }

                f2_1 = MathHelper.Max(f1_1, f2_1);
                f2_2 = MathHelper.Min(f1_2, f2_2);
                if ((double) f2_1 > (double) f2_2)
                    return new float?();
            }

            if ((double) Math.Abs(ray.Direction.Y) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.Y < (double) this.Y || (double) ray.Position.Y > (double) this.Bottom)
                    return new float?();
            }
            else
            {
                float num1 = 1f / ray.Direction.Y;
                float f1_1 = (this.Y - ray.Position.Y) * num1;
                float f1_2 = (this.Bottom - ray.Position.Y) * num1;
                if ((double) f1_1 > (double) f1_2)
                {
                    float num2 = f1_1;
                    f1_1 = f1_2;
                    f1_2 = num2;
                }

                f2_1 = MathHelper.Max(f1_1, f2_1);
                float num3 = MathHelper.Min(f1_2, f2_2);
                if ((double) f2_1 > (double) num3)
                    return new float?();
            }

            return new float?(f2_1);
        }

        public bool IntersectsWith(Point p1, Point p2)
        {
            float num1 = p1.X;
            float num2 = p2.X;
            if ((double) p1.X > (double) p2.X)
            {
                num1 = p2.X;
                num2 = p1.X;
            }

            if ((double) num2 > (double) this.Right)
                num2 = this.Right;
            if ((double) num1 < (double) this.Left)
                num1 = this.Left;
            if ((double) num1 > (double) num2)
                return false;
            float num3 = p1.Y;
            float num4 = p2.Y;
            float num5 = p2.X - p1.X;
            if ((double) Math.Abs(num5) > 1E-06)
            {
                float num6 = (p2.Y - p1.Y) / num5;
                float num7 = p1.Y - num6 * p1.X;
                num3 = num6 * num1 + num7;
                num4 = num6 * num2 + num7;
            }

            if ((double) num3 > (double) num4)
            {
                float num6 = num4;
                num4 = num3;
                num3 = num6;
            }

            if ((double) num4 > (double) this.Bottom)
                num4 = this.Bottom;
            if ((double) num3 < (double) this.Top)
                num3 = this.Top;
            return (double) num3 <= (double) num4;
        }

        public void Intersect(Rect rect)
        {
            if (!this.IntersectsWith(rect))
            {
                this = Rect.Empty;
            }
            else
            {
                float num1 = Math.Max(this.Left, rect.Left);
                float num2 = Math.Max(this.Top, rect.Top);
                this.width = Math.Max(Math.Min(this.Right, rect.Right) - num1, 0.0f);
                this.height = Math.Max(Math.Min(this.Bottom, rect.Bottom) - num2, 0.0f);
                this.x = num1;
                this.y = num2;
            }
        }

        public static Rect Intersect(Rect rect1, Rect rect2)
        {
            rect1.Intersect(rect2);
            return rect1;
        }

        public bool ClipRay(Ray2D ray2d, out Vector2 new1, out Vector2 new2)
        {
            return this.ClipLine(ray2d.Position, ray2d.Position + ray2d.Direction * float.MaxValue, out new1, out new2);
        }

        public bool ClipLine(Vector2 point1, Vector2 point2, out Vector2 new1, out Vector2 new2)
        {
            new1 = point1;
            new2 = point2;
            float num1 = point1.X;
            float num2 = point1.Y;
            float num3 = point2.X;
            float num4 = point2.Y;
            float x = this.x;
            float y = this.y;
            float num5 = this.x + this.height;
            float num6 = this.y + this.height;
            OutFlags outFlags1 = this.CalcOutFlags(num1, num2);
            OutFlags outFlags2 = this.CalcOutFlags(num3, num4);
            while ((outFlags1 | outFlags2) != OutFlags.None)
            {
                if ((outFlags1 & outFlags2) != OutFlags.None)
                    return false;
                float num7 = num3 - num1;
                float num8 = num4 - num2;
                if (outFlags1 != OutFlags.None)
                {
                    if ((outFlags1 & OutFlags.Left) == OutFlags.Left && (double) num7 != 0.0)
                    {
                        num2 += (x - num1) * num8 / num7;
                        num1 = x;
                    }
                    else if ((outFlags1 & OutFlags.Right) == OutFlags.Right && (double) num7 != 0.0)
                    {
                        num2 += (num5 - num1) * num8 / num7;
                        num1 = num5;
                    }
                    else if ((outFlags1 & OutFlags.Bottom) == OutFlags.Bottom && (double) num8 != 0.0)
                    {
                        num1 += (num6 - num2) * num7 / num8;
                        num2 = num6;
                    }
                    else if ((outFlags1 & OutFlags.Top) == OutFlags.Top && (double) num8 != 0.0)
                    {
                        num1 += (y - num2) * num7 / num8;
                        num2 = y;
                    }

                    outFlags1 = this.CalcOutFlags(num1, num2);
                }
                else if (outFlags2 != OutFlags.None)
                {
                    if ((outFlags2 & OutFlags.Left) == OutFlags.Left && (double) num7 != 0.0)
                    {
                        num4 += (x - num3) * num8 / num7;
                        num3 = x;
                    }
                    else if ((outFlags2 & OutFlags.Right) == OutFlags.Right && (double) num7 != 0.0)
                    {
                        num4 += (num5 - num3) * num8 / num7;
                        num3 = num5;
                    }
                    else if ((outFlags2 & OutFlags.Bottom) == OutFlags.Bottom && (double) num8 != 0.0)
                    {
                        num3 += (num6 - num4) * num7 / num8;
                        num4 = num6;
                    }
                    else if ((outFlags2 & OutFlags.Top) == OutFlags.Top && (double) num8 != 0.0)
                    {
                        num3 += (y - num4) * num7 / num8;
                        num4 = y;
                    }

                    outFlags2 = this.CalcOutFlags(num3, num4);
                }
            }

            new1 = new Vector2(num1, num2);
            new2 = new Vector2(num3, num4);
            return true;
        }

        private OutFlags CalcOutFlags(float x1, float y1)
        {
            OutFlags outFlags = OutFlags.None;
            if ((double) x1 < (double) this.Left)
                outFlags |= OutFlags.Left;
            else if ((double) x1 > (double) this.Right)
                outFlags |= OutFlags.Right;
            if ((double) y1 < (double) this.Top)
                outFlags |= OutFlags.Top;
            else if ((double) y1 > (double) this.Bottom)
                outFlags |= OutFlags.Bottom;
            return outFlags;
        }

        public void Union(Rect rect)
        {
            if (this.IsEmpty)
            {
                this = rect;
            }
            else
            {
                if (rect.IsEmpty)
                    return;
                float num1 = Math.Min(this.Left, rect.Left);
                float num2 = Math.Min(this.Top, rect.Top);
                this.width = !float.IsPositiveInfinity(rect.Width) && !float.IsPositiveInfinity(this.Width)
                    ? Math.Max(Math.Max(this.Right, rect.Right) - num1, 0.0f)
                    : float.PositiveInfinity;
                this.height = !float.IsPositiveInfinity(rect.Height) && !float.IsPositiveInfinity(this.Height)
                    ? Math.Max(Math.Max(this.Bottom, rect.Bottom) - num2, 0.0f)
                    : float.PositiveInfinity;
                this.x = num1;
                this.y = num2;
            }
        }

        public static Rect Union(Rect rect1, Rect rect2)
        {
            rect1.Union(rect2);
            return rect1;
        }

        public void Union(Point point)
        {
            this.Union(new Rect(point, point));
        }

        public static Rect Union(Rect rect, Point point)
        {
            rect.Union(new Rect(point, point));
            return rect;
        }

        public void Offset(float offsetX, float offsetY)
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("Cannot offset empty Rect.");
            this.x += offsetX;
            this.y += offsetY;
        }

        public static Rect Offset(Rect rect, float offsetX, float offsetY)
        {
            rect.Offset(offsetX, offsetY);
            return rect;
        }

        public void Inflate(Size size)
        {
            this.Inflate(size.Width, size.Height);
        }

        public void Inflate(float w, float h)
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("Cannot inflate empty Rect.");
            this.x -= w;
            this.y -= h;
            this.width += w;
            this.width += w;
            this.height += h;
            this.height += h;
            if ((double) this.width >= 0.0 && (double) this.height >= 0.0)
                return;
            this = Rect.s_empty;
        }

        public static Rect Inflate(Rect rect, Size size)
        {
            rect.Inflate(size.Width, size.Height);
            return rect;
        }

        public static Rect Inflate(Rect rect, float width, float height)
        {
            rect.Inflate(width, height);
            return rect;
        }

        public void Scale(float scaleX, float scaleY)
        {
            if (this.IsEmpty)
                return;
            this.x *= scaleX;
            this.y *= scaleY;
            this.width *= scaleX;
            this.height *= scaleY;
            if ((double) scaleX < 0.0)
            {
                this.x += this.width;
                this.width *= -1f;
            }

            if ((double) scaleY < 0.0)
            {
                this.y += this.height;
                this.height *= -1f;
            }
        }

        private bool ContainsInternal(float xPos, float yPos)
        {
            return (double) xPos >= (double) this.x && (double) xPos - (double) this.height <= (double) this.x &&
                   (double) yPos >= (double) this.y && (double) yPos - (double) this.width <= (double) this.y;
        }
    }
}