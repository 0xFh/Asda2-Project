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
            get { return this.text; }
        }

        public Bitmap Image
        {
            get { return this.image; }
        }

        public int Width
        {
            get { return this.width; }
        }

        public int Height
        {
            get { return this.height; }
        }

        public CaptchaImage(string s, int width, int height)
        {
            this.text = s;
            this.SetDimensions(width, height);
            this.GenerateImage();
        }

        public CaptchaImage(string s, int width, int height, string familyName)
        {
            this.text = s;
            this.SetDimensions(width, height);
            this.SetFamilyName(familyName);
            this.GenerateImage();
        }

        ~CaptchaImage()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize((object) this);
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            this.image.Dispose();
        }

        private void SetDimensions(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), (object) width,
                    "Argument out of range, must be greater than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), (object) height,
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
            catch (Exception ex)
            {
                this.familyName = FontFamily.GenericSerif.Name;
            }
        }

        private void GenerateImage()
        {
            Bitmap bitmap = new Bitmap(this.width, this.height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage((System.Drawing.Image) bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rectangle = new Rectangle(0, 0, this.width, this.height);
            HatchBrush hatchBrush1 = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White);
            graphics.FillRectangle((Brush) hatchBrush1, rectangle);
            float emSize = (float) (rectangle.Height + 1);
            Font font;
            do
            {
                --emSize;
                font = new Font(this.familyName, emSize, FontStyle.Bold);
            } while ((double) graphics.MeasureString(this.text, font).Width > (double) rectangle.Width);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            GraphicsPath path = new GraphicsPath();
            path.AddString(this.text, font.FontFamily, (int) font.Style, font.Size, rectangle, format);
            float num1 = 4f;
            PointF[] destPoints = new PointF[4]
            {
                new PointF((float) this.random.Next(rectangle.Width) / num1,
                    (float) this.random.Next(rectangle.Height) / num1),
                new PointF((float) rectangle.Width - (float) this.random.Next(rectangle.Width) / num1,
                    (float) this.random.Next(rectangle.Height) / num1),
                new PointF((float) this.random.Next(rectangle.Width) / num1,
                    (float) rectangle.Height - (float) this.random.Next(rectangle.Height) / num1),
                new PointF((float) rectangle.Width - (float) this.random.Next(rectangle.Width) / num1,
                    (float) rectangle.Height - (float) this.random.Next(rectangle.Height) / num1)
            };
            Matrix matrix = new Matrix();
            matrix.Translate(0.0f, 0.0f);
            path.Warp(destPoints, (RectangleF) rectangle, matrix, WarpMode.Perspective, 0.0f);
            HatchBrush hatchBrush2 = new HatchBrush(HatchStyle.LargeConfetti, Color.LightGray, Color.DarkGray);
            graphics.FillPath((Brush) hatchBrush2, path);
            int num2 = Math.Max(rectangle.Width, rectangle.Height);
            for (int index = 0; index < (int) ((double) (rectangle.Width * rectangle.Height) / 30.0); ++index)
            {
                int x = this.random.Next(rectangle.Width);
                int y = this.random.Next(rectangle.Height);
                int width = this.random.Next(num2 / 50);
                int height = this.random.Next(num2 / 50);
                graphics.FillEllipse((Brush) hatchBrush2, x, y, width, height);
            }

            font.Dispose();
            hatchBrush2.Dispose();
            graphics.Dispose();
            this.image = bitmap;
        }
    }
}