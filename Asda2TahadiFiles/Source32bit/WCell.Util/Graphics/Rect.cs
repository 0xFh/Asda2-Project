using System;

namespace WCell.Util.Graphics
{
  /// <summary>
  /// Represents a Rectangle in WoW coordinate space
  /// i.e. X-positive into the screen, Y-Positive to the left
  /// </summary>
  public struct Rect
  {
    private static readonly Rect s_empty = new Rect
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
      return rect1.X == (double) rect2.X && rect1.Y == (double) rect2.Y &&
             rect1.Width == (double) rect2.Width && rect1.Height == (double) rect2.Height;
    }

    public static bool operator !=(Rect rect1, Rect rect2)
    {
      return !(rect1 == rect2);
    }

    public static bool Equals(Rect rect1, Rect rect2)
    {
      if(rect1.IsEmpty)
        return rect2.IsEmpty;
      return rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Width.Equals(rect2.Width) &&
             rect1.Height.Equals(rect2.Height);
    }

    public override bool Equals(object o)
    {
      if(o == null || !(o is Rect))
        return false;
      return Equals(this, (Rect) o);
    }

    public bool Equals(Rect value)
    {
      return Equals(this, value);
    }

    public override int GetHashCode()
    {
      if(IsEmpty)
        return 0;
      int hashCode1 = X.GetHashCode();
      float num1 = Y;
      int hashCode2 = num1.GetHashCode();
      int num2 = hashCode1 ^ hashCode2;
      num1 = Width;
      int hashCode3 = num1.GetHashCode();
      int num3 = num2 ^ hashCode3;
      num1 = Height;
      int hashCode4 = num1.GetHashCode();
      return num3 ^ hashCode4;
    }

    public override string ToString()
    {
      if(IsEmpty)
        return "Empty";
      return string.Format("X: {0}, Y: {1}, Width: {2}, Height: {3}", (object) x, (object) y,
        (object) width, (object) height);
    }

    public Rect(Point location, Size size)
    {
      if(size.IsEmpty)
      {
        this = s_empty;
      }
      else
      {
        x = location.X;
        y = location.Y;
        width = size.Width;
        height = size.Height;
      }
    }

    public Rect(float x, float y, float width, float height)
    {
      if(width < 0.0 || height < 0.0)
        throw new ArgumentException("Width and Height cannot be negative");
      this.x = x;
      this.y = y;
      this.width = width;
      this.height = height;
    }

    public Rect(Point point1, Point point2)
    {
      x = Math.Min(point1.X, point2.X);
      y = Math.Min(point1.Y, point2.Y);
      width = Math.Max(Math.Max(point1.X, point2.X) - x, 0.0f);
      height = Math.Max(Math.Max(point1.Y, point2.Y) - y, 0.0f);
    }

    public Rect(Size size)
    {
      if(size.IsEmpty)
      {
        this = s_empty;
      }
      else
      {
        x = y = 0.0f;
        width = size.Width;
        height = size.Height;
      }
    }

    public static Rect Empty
    {
      get { return s_empty; }
    }

    public bool IsEmpty
    {
      get { return width < 0.0; }
    }

    public Point Location
    {
      get { return new Point(x, y); }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty Rect.");
        x = value.X;
        y = value.Y;
      }
    }

    public Size Size
    {
      get
      {
        if(IsEmpty)
          return Size.Empty;
        return new Size(width, height);
      }
      set
      {
        if(value.IsEmpty)
        {
          this = s_empty;
        }
        else
        {
          if(IsEmpty)
            throw new InvalidOperationException("Cannot modify empty Rect.");
          width = value.Width;
          height = value.Height;
        }
      }
    }

    public float X
    {
      get { return x; }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty Rect.");
        x = value;
      }
    }

    public float Y
    {
      get { return y; }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty Rect.");
        y = value;
      }
    }

    public float Width
    {
      get { return width; }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty Rect.");
        if(value < 0.0)
          throw new ArgumentException("Width cannot be negative.");
        width = value;
      }
    }

    public float Height
    {
      get { return height; }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty Rect.");
        if(value < 0.0)
          throw new ArgumentException("Height cannot be negative");
        height = value;
      }
    }

    public float Left
    {
      get
      {
        if(IsEmpty)
          return float.NegativeInfinity;
        return y + width;
      }
    }

    public float Top
    {
      get
      {
        if(IsEmpty)
          return float.NegativeInfinity;
        return x + height;
      }
    }

    public float Right
    {
      get { return y; }
    }

    public float Bottom
    {
      get { return x; }
    }

    public Point TopLeft
    {
      get { return new Point(Top, Left); }
    }

    public Point TopRight
    {
      get { return new Point(Top, Right); }
    }

    public Point BottomLeft
    {
      get { return new Point(Bottom, Left); }
    }

    public Point BottomRight
    {
      get { return new Point(Bottom, Right); }
    }

    /// <summary>
    /// Whether the rectangle contains the given Point.
    /// Points laying on the rectangles border are considered to be contained.
    /// </summary>
    public bool Contains(Point point)
    {
      return Contains(point.X, point.Y);
    }

    /// <summary>
    /// Whether the rectangle contains the given Point(x, y).
    /// Points laying on the rectangles border are considered to be contained.
    /// </summary>
    public bool Contains(float xPos, float yPos)
    {
      if(IsEmpty)
        return false;
      return ContainsInternal(xPos, yPos);
    }

    public bool Contains(Rect rect)
    {
      if(IsEmpty || rect.IsEmpty)
        return false;
      return x <= (double) rect.x && y <= (double) rect.y &&
             x + (double) height >= rect.x + (double) rect.height &&
             y + (double) width >= rect.y + (double) rect.width;
    }

    public bool IntersectsWith(Rect rect)
    {
      if(IsEmpty || rect.IsEmpty)
        return false;
      return rect.Left <= (double) Right && rect.Right >= (double) Left &&
             rect.Top <= (double) Bottom && rect.Bottom >= (double) Top;
    }

    public bool IntersectsWith(Ray2D ray)
    {
      return IntersectWith(ray).HasValue;
    }

    public float? IntersectWith(Ray2D ray)
    {
      float f2_1 = 0.0f;
      float f2_2 = float.MaxValue;
      if(Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
      {
        if(ray.Position.X < (double) X || ray.Position.X > (double) Right)
          return new float?();
      }
      else
      {
        float num1 = 1f / ray.Direction.X;
        float f1_1 = (X - ray.Position.X) * num1;
        float f1_2 = (Right - ray.Position.X) * num1;
        if(f1_1 > (double) f1_2)
        {
          float num2 = f1_1;
          f1_1 = f1_2;
          f1_2 = num2;
        }

        f2_1 = MathHelper.Max(f1_1, f2_1);
        f2_2 = MathHelper.Min(f1_2, f2_2);
        if(f2_1 > (double) f2_2)
          return new float?();
      }

      if(Math.Abs(ray.Direction.Y) < 9.99999997475243E-07)
      {
        if(ray.Position.Y < (double) Y || ray.Position.Y > (double) Bottom)
          return new float?();
      }
      else
      {
        float num1 = 1f / ray.Direction.Y;
        float f1_1 = (Y - ray.Position.Y) * num1;
        float f1_2 = (Bottom - ray.Position.Y) * num1;
        if(f1_1 > (double) f1_2)
        {
          float num2 = f1_1;
          f1_1 = f1_2;
          f1_2 = num2;
        }

        f2_1 = MathHelper.Max(f1_1, f2_1);
        float num3 = MathHelper.Min(f1_2, f2_2);
        if(f2_1 > (double) num3)
          return new float?();
      }

      return f2_1;
    }

    public bool IntersectsWith(Point p1, Point p2)
    {
      float num1 = p1.X;
      float num2 = p2.X;
      if(p1.X > (double) p2.X)
      {
        num1 = p2.X;
        num2 = p1.X;
      }

      if(num2 > (double) Right)
        num2 = Right;
      if(num1 < (double) Left)
        num1 = Left;
      if(num1 > (double) num2)
        return false;
      float num3 = p1.Y;
      float num4 = p2.Y;
      float num5 = p2.X - p1.X;
      if(Math.Abs(num5) > 1E-06)
      {
        float num6 = (p2.Y - p1.Y) / num5;
        float num7 = p1.Y - num6 * p1.X;
        num3 = num6 * num1 + num7;
        num4 = num6 * num2 + num7;
      }

      if(num3 > (double) num4)
      {
        float num6 = num4;
        num4 = num3;
        num3 = num6;
      }

      if(num4 > (double) Bottom)
        num4 = Bottom;
      if(num3 < (double) Top)
        num3 = Top;
      return num3 <= (double) num4;
    }

    public void Intersect(Rect rect)
    {
      if(!IntersectsWith(rect))
      {
        this = Empty;
      }
      else
      {
        float num1 = Math.Max(Left, rect.Left);
        float num2 = Math.Max(Top, rect.Top);
        width = Math.Max(Math.Min(Right, rect.Right) - num1, 0.0f);
        height = Math.Max(Math.Min(Bottom, rect.Bottom) - num2, 0.0f);
        x = num1;
        y = num2;
      }
    }

    public static Rect Intersect(Rect rect1, Rect rect2)
    {
      rect1.Intersect(rect2);
      return rect1;
    }

    public bool ClipRay(Ray2D ray2d, out Vector2 new1, out Vector2 new2)
    {
      return ClipLine(ray2d.Position, ray2d.Position + ray2d.Direction * float.MaxValue, out new1, out new2);
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
      float num5 = this.x + height;
      float num6 = this.y + height;
      OutFlags outFlags1 = CalcOutFlags(num1, num2);
      OutFlags outFlags2 = CalcOutFlags(num3, num4);
      while((outFlags1 | outFlags2) != OutFlags.None)
      {
        if((outFlags1 & outFlags2) != OutFlags.None)
          return false;
        float num7 = num3 - num1;
        float num8 = num4 - num2;
        if(outFlags1 != OutFlags.None)
        {
          if((outFlags1 & OutFlags.Left) == OutFlags.Left && num7 != 0.0)
          {
            num2 += (x - num1) * num8 / num7;
            num1 = x;
          }
          else if((outFlags1 & OutFlags.Right) == OutFlags.Right && num7 != 0.0)
          {
            num2 += (num5 - num1) * num8 / num7;
            num1 = num5;
          }
          else if((outFlags1 & OutFlags.Bottom) == OutFlags.Bottom && num8 != 0.0)
          {
            num1 += (num6 - num2) * num7 / num8;
            num2 = num6;
          }
          else if((outFlags1 & OutFlags.Top) == OutFlags.Top && num8 != 0.0)
          {
            num1 += (y - num2) * num7 / num8;
            num2 = y;
          }

          outFlags1 = CalcOutFlags(num1, num2);
        }
        else if(outFlags2 != OutFlags.None)
        {
          if((outFlags2 & OutFlags.Left) == OutFlags.Left && num7 != 0.0)
          {
            num4 += (x - num3) * num8 / num7;
            num3 = x;
          }
          else if((outFlags2 & OutFlags.Right) == OutFlags.Right && num7 != 0.0)
          {
            num4 += (num5 - num3) * num8 / num7;
            num3 = num5;
          }
          else if((outFlags2 & OutFlags.Bottom) == OutFlags.Bottom && num8 != 0.0)
          {
            num3 += (num6 - num4) * num7 / num8;
            num4 = num6;
          }
          else if((outFlags2 & OutFlags.Top) == OutFlags.Top && num8 != 0.0)
          {
            num3 += (y - num4) * num7 / num8;
            num4 = y;
          }

          outFlags2 = CalcOutFlags(num3, num4);
        }
      }

      new1 = new Vector2(num1, num2);
      new2 = new Vector2(num3, num4);
      return true;
    }

    private OutFlags CalcOutFlags(float x1, float y1)
    {
      OutFlags outFlags = OutFlags.None;
      if(x1 < (double) Left)
        outFlags |= OutFlags.Left;
      else if(x1 > (double) Right)
        outFlags |= OutFlags.Right;
      if(y1 < (double) Top)
        outFlags |= OutFlags.Top;
      else if(y1 > (double) Bottom)
        outFlags |= OutFlags.Bottom;
      return outFlags;
    }

    public void Union(Rect rect)
    {
      if(IsEmpty)
      {
        this = rect;
      }
      else
      {
        if(rect.IsEmpty)
          return;
        float num1 = Math.Min(Left, rect.Left);
        float num2 = Math.Min(Top, rect.Top);
        width = !float.IsPositiveInfinity(rect.Width) && !float.IsPositiveInfinity(Width)
          ? Math.Max(Math.Max(Right, rect.Right) - num1, 0.0f)
          : float.PositiveInfinity;
        height = !float.IsPositiveInfinity(rect.Height) && !float.IsPositiveInfinity(Height)
          ? Math.Max(Math.Max(Bottom, rect.Bottom) - num2, 0.0f)
          : float.PositiveInfinity;
        x = num1;
        y = num2;
      }
    }

    public static Rect Union(Rect rect1, Rect rect2)
    {
      rect1.Union(rect2);
      return rect1;
    }

    public void Union(Point point)
    {
      Union(new Rect(point, point));
    }

    public static Rect Union(Rect rect, Point point)
    {
      rect.Union(new Rect(point, point));
      return rect;
    }

    public void Offset(float offsetX, float offsetY)
    {
      if(IsEmpty)
        throw new InvalidOperationException("Cannot offset empty Rect.");
      x += offsetX;
      y += offsetY;
    }

    public static Rect Offset(Rect rect, float offsetX, float offsetY)
    {
      rect.Offset(offsetX, offsetY);
      return rect;
    }

    public void Inflate(Size size)
    {
      Inflate(size.Width, size.Height);
    }

    public void Inflate(float w, float h)
    {
      if(IsEmpty)
        throw new InvalidOperationException("Cannot inflate empty Rect.");
      x -= w;
      y -= h;
      width += w;
      width += w;
      height += h;
      height += h;
      if(width >= 0.0 && height >= 0.0)
        return;
      this = s_empty;
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
      if(IsEmpty)
        return;
      x *= scaleX;
      y *= scaleY;
      width *= scaleX;
      height *= scaleY;
      if(scaleX < 0.0)
      {
        x += width;
        width *= -1f;
      }

      if(scaleY < 0.0)
      {
        y += height;
        height *= -1f;
      }
    }

    private bool ContainsInternal(float xPos, float yPos)
    {
      return xPos >= (double) x && xPos - (double) height <= x &&
             yPos >= (double) y && yPos - (double) width <= y;
    }
  }
}