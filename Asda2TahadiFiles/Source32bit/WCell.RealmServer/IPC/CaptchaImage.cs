using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace WCell.RealmServer.IPC
{
  /// <summary>Summary description for CaptchaImage.</summary>
  public class CaptchaImage
  {
    private Random random = new Random();
    private string text;
    private int width;
    private int height;
    private string familyName;
    private Bitmap image;

    public string Text
    {
      get { return text; }
    }

    public Bitmap Image
    {
      get { return image; }
    }

    public int Width
    {
      get { return width; }
    }

    public int Height
    {
      get { return height; }
    }

    public CaptchaImage(string s, int width, int height)
    {
      text = s;
      SetDimensions(width, height);
      GenerateImage();
    }

    public CaptchaImage(string s, int width, int height, string familyName)
    {
      text = s;
      SetDimensions(width, height);
      SetFamilyName(familyName);
      GenerateImage();
    }

    ~CaptchaImage()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
      if(!disposing)
        return;
      image.Dispose();
    }

    private void SetDimensions(int width, int height)
    {
      if(width <= 0)
        throw new ArgumentOutOfRangeException(nameof(width), width,
          "Argument out of range, must be greater than zero.");
      if(height <= 0)
        throw new ArgumentOutOfRangeException(nameof(height), height,
          "Argument out of range, must be greater than zero.");
      this.width = width;
      this.height = height;
    }

    private void SetFamilyName(string familyName)
    {
      try
      {
        Font font = new Font(this.familyName, 12f);
        this.familyName = familyName;
        font.Dispose();
      }
      catch(Exception ex)
      {
        this.familyName = FontFamily.GenericSerif.Name;
      }
    }

    private void GenerateImage()
    {
      Bitmap bitmap = new Bitmap(this.width, this.height, PixelFormat.Format32bppArgb);
      Graphics graphics = Graphics.FromImage(bitmap);
      graphics.SmoothingMode = SmoothingMode.AntiAlias;
      Rectangle rectangle = new Rectangle(0, 0, this.width, this.height);
      HatchBrush hatchBrush1 = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White);
      graphics.FillRectangle(hatchBrush1, rectangle);
      float emSize = rectangle.Height + 1;
      Font font;
      do
      {
        --emSize;
        font = new Font(familyName, emSize, FontStyle.Bold);
      } while(graphics.MeasureString(text, font).Width > (double) rectangle.Width);

      StringFormat format = new StringFormat();
      format.Alignment = StringAlignment.Center;
      format.LineAlignment = StringAlignment.Center;
      GraphicsPath path = new GraphicsPath();
      path.AddString(text, font.FontFamily, (int) font.Style, font.Size, rectangle, format);
      float num1 = 4f;
      PointF[] destPoints = new PointF[4]
      {
        new PointF(random.Next(rectangle.Width) / num1,
          random.Next(rectangle.Height) / num1),
        new PointF(rectangle.Width - random.Next(rectangle.Width) / num1,
          random.Next(rectangle.Height) / num1),
        new PointF(random.Next(rectangle.Width) / num1,
          rectangle.Height - random.Next(rectangle.Height) / num1),
        new PointF(rectangle.Width - random.Next(rectangle.Width) / num1,
          rectangle.Height - random.Next(rectangle.Height) / num1)
      };
      Matrix matrix = new Matrix();
      matrix.Translate(0.0f, 0.0f);
      path.Warp(destPoints, rectangle, matrix, WarpMode.Perspective, 0.0f);
      HatchBrush hatchBrush2 = new HatchBrush(HatchStyle.LargeConfetti, Color.LightGray, Color.DarkGray);
      graphics.FillPath(hatchBrush2, path);
      int num2 = Math.Max(rectangle.Width, rectangle.Height);
      for(int index = 0; index < (int) ((double) (rectangle.Width * rectangle.Height) / 30.0); ++index)
      {
        int x = random.Next(rectangle.Width);
        int y = random.Next(rectangle.Height);
        int width = random.Next(num2 / 50);
        int height = random.Next(num2 / 50);
        graphics.FillEllipse(hatchBrush2, x, y, width, height);
      }

      font.Dispose();
      hatchBrush2.Dispose();
      graphics.Dispose();
      image = bitmap;
    }
  }
}