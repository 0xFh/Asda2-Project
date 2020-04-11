using System;

namespace WCell.Util.Graphics
{
    public class RectInt32
    {
        private static readonly RectInt32 s_empty = new RectInt32()
        {
            x = 0,
            y = 0,
            width = 0,
            height = 0
        };

        internal int x;
        internal int y;
        internal int width;
        internal int height;

        public static bool operator ==(RectInt32 rect1, RectInt32 rect2)
        {
            return rect1.X == rect2.X && rect1.Y == rect2.Y && rect1.Width == rect2.Width &&
                   rect1.Height == rect2.Height;
        }

        public static bool operator !=(RectInt32 rect1, RectInt32 rect2)
        {
            return !(rect1 == rect2);
        }

        public static bool Equals(RectInt32 rect1, RectInt32 rect2)
        {
            if (rect1.IsEmpty)
                return rect2.IsEmpty;
            return rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Width.Equals(rect2.Width) &&
                   rect1.Height.Equals(rect2.Height);
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is RectInt32))
                return false;
            return RectInt32.Equals(this, (RectInt32) o);
        }

        public bool Equals(RectInt32 value)
        {
            return RectInt32.Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (this.IsEmpty)
                return 0;
            int hashCode1 = this.X.GetHashCode();
            int num1 = this.Y;
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

        public RectInt32(int x, int y, int width, int height)
        {
            if ((double) width < 0.0 || (double) height < 0.0)
                throw new ArgumentException("Width and Height cannot be negative");
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        private RectInt32()
        {
        }

        public static RectInt32 Empty
        {
            get { return RectInt32.s_empty; }
        }

        public bool IsEmpty
        {
            get { return (double) this.width < 0.0; }
        }

        public int X
        {
            get { return this.x; }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty RectInt32.");
                this.x = value;
            }
        }

        public int Y
        {
            get { return this.y; }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty RectInt32.");
                this.y = value;
            }
        }

        public int Width
        {
            get { return this.width; }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty RectInt32.");
                if ((double) value < 0.0)
                    throw new ArgumentException("Width cannot be negative.");
                this.width = value;
            }
        }

        public int Height
        {
            get { return this.height; }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Cannot modify empty RectInt32.");
                if ((double) value < 0.0)
                    throw new ArgumentException("Height cannot be negative");
                this.height = value;
            }
        }

        public int Left
        {
            get { return this.x; }
        }

        public int Top
        {
            get { return this.y; }
        }

        public int Right
        {
            get { return this.x + this.width; }
        }

        public int Bottom
        {
            get { return this.y + this.height; }
        }

        public Point TopLeft
        {
            get { return new Point((float) this.Left, (float) this.Top); }
        }

        public Point TopRight
        {
            get { return new Point((float) this.Right, (float) this.Top); }
        }

        public Point BottomLeft
        {
            get { return new Point((float) this.Left, (float) this.Bottom); }
        }

        public Point BottomRight
        {
            get { return new Point((float) this.Right, (float) this.Bottom); }
        }

        /// <summary>
        /// Whether the rectangle contains the given Point(x, y).
        /// Points laying on the rectangles border are considered to be contained.
        /// </summary>
        public bool Contains(int xPos, int yPos)
        {
            if (this.IsEmpty)
                return false;
            return this.ContainsInternal(xPos, yPos);
        }

        public bool Contains(RectInt32 rect)
        {
            if (this.IsEmpty || rect.IsEmpty)
                return false;
            return this.x <= rect.x && this.y <= rect.y && this.x + this.width >= rect.x + rect.width &&
                   this.y + this.height >= rect.y + rect.height;
        }

        public bool IntersectsWith(RectInt32 rect)
        {
            if (this.IsEmpty || rect.IsEmpty)
                return false;
            return rect.Left <= this.Right && rect.Right >= this.Left && rect.Top <= this.Bottom &&
                   rect.Bottom >= this.Top;
        }

        public bool IntersectsWith(Ray2D ray)
        {
            return this.IntersectWith(ray).HasValue;
        }

        public float? IntersectWith(Ray2D ray)
        {
            float f2_1 = 0.0f;
            float f2_2 = (float) int.MaxValue;
            if ((double) Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.X < (double) this.X || (double) ray.Position.X > (double) this.Right)
                    return new float?();
            }
            else
            {
                float num1 = 1f / ray.Direction.X;
                float f1_1 = ((float) this.X - ray.Position.X) * num1;
                float f1_2 = ((float) this.Right - ray.Position.X) * num1;
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
                float f1_1 = ((float) this.Y - ray.Position.Y) * num1;
                float f1_2 = ((float) this.Bottom - ray.Position.Y) * num1;
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
                num2 = (float) this.Right;
            if ((double) num1 < (double) this.Left)
                num1 = (float) this.Left;
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
                num4 = (float) this.Bottom;
            if ((double) num3 < (double) this.Top)
                num3 = (float) this.Top;
            return (double) num3 <= (double) num4;
        }

        public RectInt32 Intersect(RectInt32 rect)
        {
            if (!this.IntersectsWith(rect))
                return RectInt32.Empty;
            int num1 = Math.Max(this.Left, rect.Left);
            int num2 = Math.Max(this.Top, rect.Top);
            return new RectInt32()
            {
                width = Math.Max(Math.Min(this.Right, rect.Right) - num1, 0),
                height = Math.Max(Math.Min(this.Bottom, rect.Bottom) - num2, 0),
                x = num1,
                y = num2
            };
        }

        public static RectInt32 Intersect(RectInt32 rect1, RectInt32 rect2)
        {
            rect1.Intersect(rect2);
            return rect1;
        }

        public void Offset(int offsetX, int offsetY)
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("Cannot offset empty RectInt32.");
            this.x += offsetX;
            this.y += offsetY;
        }

        public static RectInt32 Offset(RectInt32 rect, int offsetX, int offsetY)
        {
            rect.Offset(offsetX, offsetY);
            return rect;
        }

        public void Inflate(int w, int h)
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("Cannot inflate empty RectInt32.");
            this.x -= w;
            this.y -= h;
            this.width += w;
            this.width += w;
            this.height += h;
            this.height += h;
        }

        public void Scale(int scaleX, int scaleY)
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
                this.width *= -1;
            }

            if ((double) scaleY < 0.0)
            {
                this.y += this.height;
                this.height *= -1;
            }
        }

        private bool ContainsInternal(int xPos, int yPos)
        {
            return xPos >= this.x && xPos - this.width <= this.x && yPos >= this.y && yPos - this.height <= this.y;
        }
    }
}