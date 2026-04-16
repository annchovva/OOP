using MyPaint.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MyPaint.Plugins
{
    public class Ellipse : IFigure
    {
        public string Name { get; } = "Ellipse";
        public Color Color { get; set; }
        public Color BorderColor { get; set; }
        public int Thickness { get; set; }
        public float Angle { get; set; } = 0;

        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private int X;
        private int Y;
        public void OnMouseDown(int x, int y)
        {
            X = x;
            Y = y;

            StartX = x;
            StartY = y;
        }
        public void OnMouseUp(int x, int y, Graphics graphics)
        {
            OnMouseMove(x, y, graphics);
        }
        public void OnMouseMove(int x, int y, Graphics graphics)
        {
            int height = Math.Abs(y - Y);
            int width = Math.Abs(x - X);
            int x_start = x > X ? X : X - width;
            int y_start = y > Y ? Y : Y - height;

            StartX = x_start;
            StartY = y_start;
            Width = width;
            Height = height;


            using (Brush brush = new SolidBrush(this.Color))
            {
                graphics.FillEllipse(brush, x_start, y_start, width, height);
            }
            using (Pen pen = new Pen(this.BorderColor, this.Thickness))
            {
                graphics.DrawEllipse(pen, x_start, y_start, width, height);
            }

        }

        public void Draw(Graphics graphics)
        {
            var save = graphics.Save();
            var CenterX = StartX + Width / 2;
            var CenterY = StartY + Height / 2;

            graphics.TranslateTransform(CenterX, CenterY);
            graphics.RotateTransform(Angle);

            var half_x = Width / 2;
            var half_y = Height / 2;

            using (Brush brush = new SolidBrush(this.Color))
            {
                graphics.FillEllipse(brush, -half_x, -half_y, Width, Height);
            }
            using (Pen pen = new Pen(this.BorderColor, this.Thickness))
            {
                graphics.DrawEllipse(pen, -half_x, -half_y, Width, Height);
            }

            graphics.Restore(save);
        }

        public bool IsSelected(int x, int y)
        {
            var matrix = new System.Drawing.Drawing2D.Matrix();
            var CenterX = StartX + Width / 2;
            var CenterY = StartY + Height / 2;

            matrix.RotateAt(Angle, new PointF(CenterX, CenterY));
            matrix.Invert();

            var p = new PointF[] { new PointF(x, y) };
            matrix.TransformPoints(p);


            bool bool_x = p[0].X >= StartX && p[0].X <= StartX + Width;
            bool bool_y = p[0].Y >= StartY && p[0].Y <= StartY + Height;
            return bool_x && bool_y;
        }

        public System.Drawing.Rectangle PaintBorderSelected()
        {
            return new System.Drawing.Rectangle(StartX - 10, StartY - 10, Width + 20, Height + 20);
        }

        public void Move(int delta_x, int delta_y)
        {
            StartX += delta_x;
            StartY += delta_y;

        }


        public void ChangeStartPoint(int x, int y)
        {
            if (Width - x < 5 || Height - y < 5) return;
            StartX += x;
            StartY += y;
            Width -= x;
            Height -= y;

        }
        public void ChangeEndPoint(int x, int y)
        {
            if (Width + x < 5 || Height + y < 5) return;
            Width += x;
            Height += y;

        }
        public IFigure Clone()
        {
            return (IFigure)this.MemberwiseClone();
        }
    }
}
