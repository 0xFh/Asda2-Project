using System;

namespace WCell.Util.Graphics
{
  public class RectInt32
  {
    private static readonly RectInt32 s_empty = new RectInt32
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
      if(rect1.IsEmpty)
        return rect2.IsEmpty;
      return rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Width.Equals(rect2.Width) &&
             rect1.Height.Equals(rect2.Height);
    }

    public override bool Equals(object o)
    {
      if(o == null || !(o is RectInt32))
        return false;
      return Equals(this, (RectInt32) o);
    }

    public bool Equals(RectInt32 value)
    {
      return Equals(this, value);
    }

    public override int GetHashCode()
    {
      if(IsEmpty)
        return 0;
      int hashCode1 = X.GetHashCode();
      int num1 = Y;
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

    public RectInt32(int x, int y, int width, int height)
    {
      if(width < 0.0 || height < 0.0)
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
      get { return s_empty; }
    }

    public bool IsEmpty
    {
      get { return width < 0.0; }
    }

    public int X
    {
      get { return x; }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty RectInt32.");
        x = value;
      }
    }

    public int Y
    {
      get { return y; }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty RectInt32.");
        y = value;
      }
    }

    public int Width
    {
      get { return width; }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty RectInt32.");
        if(value < 0.0)
          throw new ArgumentException("Width cannot be negative.");
        width = value;
      }
    }

    public int Height
    {
      get { return height; }
      set
      {
        if(IsEmpty)
          throw new InvalidOperationException("Cannot modify empty RectInt32.");
        if(value < 0.0)
          throw new ArgumentException("Height cannot be negative");
        height = value;
      }
    }

    public int Left
    {
      get { return x; }
    }

    public int Top
    {
      get { return y; }
    }

    public int Right
    {
      get { return x + width; }
    }

    public int Bottom
    {
      get { return y + height; }
    }

    public Point TopLeft
    {
      get { return new Point(Left, Top); }
    }

    public Point TopRight
    {
      get { return new Point(Right, Top); }
    }

    public Point BottomLeft
    {
      get { return new Point(Left, Bottom); }
    }

    public Point BottomRight
    {
      get { return new Point(Right, Bottom); }
    }

    /// <summary>
    /// Whether the rectangle contains the given Point(x, y).
    /// Points laying on the rectangles border are considered to be contained.
    /// </summary>
    public bool Contains(int xPos, int yPos)
    {
      if(IsEmpty)
        return false;
      return ContainsInternal(xPos, yPos);
    }

    public bool Contains(RectInt32 rect)
    {
      if(IsEmpty || rect.IsEmpty)
        return false;
      return x <= rect.x && y <= rect.y && x + width >= rect.x + rect.width &&
             y + height >= rect.y + rect.height;
    }

    public bool IntersectsWith(RectInt32 rect)
    {
      if(IsEmpty || rect.IsEmpty)
        return false;
      return rect.Left <= Right && rect.Right >= Left && rect.Top <= Bottom &&
             rect.Bottom >= Top;
    }

    public bool IntersectsWith(Ray2D ray)
    {
      return IntersectWith(ray).HasValue;
    }

    public float? IntersectWith(Ray2D ray)
    {
      float f2_1 = 0.0f;
      float f2_2 = int.MaxValue;
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

    public RectInt32 Intersect(RectInt32 rect)
    {
      if(!IntersectsWith(rect))
        return Empty;
      int num1 = Math.Max(Left, rect.Left);
      int num2 = Math.Max(Top, rect.Top);
      return new RectInt32
      {
        width = Math.Max(Math.Min(Right, rect.Right) - num1, 0),
        height = Math.Max(Math.Min(Bottom, rect.Bottom) - num2, 0),
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
      if(IsEmpty)
        throw new InvalidOperationException("Cannot offset empty RectInt32.");
      x += offsetX;
      y += offsetY;
    }

    public static RectInt32 Offset(RectInt32 rect, int offsetX, int offsetY)
    {
      rect.Offset(offsetX, offsetY);
      return rect;
    }

    public void Inflate(int w, int h)
    {
      if(IsEmpty)
        throw new InvalidOperationException("Cannot inflate empty RectInt32.");
      x -= w;
      y -= h;
      width += w;
      width += w;
      height += h;
      height += h;
    }

    public void Scale(int scaleX, int scaleY)
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
        width *= -1;
      }

      if(scaleY < 0.0)
      {
        y += height;
        height *= -1;
      }
    }

    private bool ContainsInternal(int xPos, int yPos)
    {
      return xPos >= x && xPos - width <= x && yPos >= y && yPos - height <= y;
    }
  }
}