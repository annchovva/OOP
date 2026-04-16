using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MyPaint.Core;

namespace MyPaint.Plugins
{
    public class Triangle : IFigure
    {
        public string Name { get; } = "Triangle";
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

        private PointF[] GetPoints()
        {
            return new PointF[]
            {
                new PointF (StartX + Width / 2, StartY),
                new PointF (StartX, StartY + Height),
                new PointF (StartX + Width, StartY + Height)
            };
        }
        public void OnMouseMove(int x, int y, Graphics graphics)
        {
            StartX = Math.Min(x, X);
            StartY = Math.Min(y, Y);
            Width = Math.Abs(x - X);
            Height = Math.Abs(y - Y);

            Draw(graphics);

        }

        public void Draw(Graphics graphics)
        {
            if (Width <= 0 || Height <= 0) return;

            var save = graphics.Save();

            float centerX = StartX + Width / 2f;
            float centerY = StartY + Height / 2f;

            graphics.TranslateTransform(centerX, centerY);
            graphics.RotateTransform(Angle);

            graphics.TranslateTransform(-centerX, -centerY);

            PointF[] points = GetPoints();

            using (Brush brush = new SolidBrush(this.Color))
            {
                graphics.FillPolygon(brush, points);
            }
            using (Pen pen = new Pen(this.BorderColor, this.Thickness))
            {
                graphics.DrawPolygon(pen, points);
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

            return PointInTriangle(p[0], GetPoints());


        }

        private bool PointInTriangle(PointF pt, PointF[] tri)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, tri[0], tri[1]);
            d2 = Sign(pt, tri[1], tri[2]);
            d3 = Sign(pt, tri[2], tri[0]);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        private float Sign(PointF p1, PointF p2, PointF p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public System.Drawing.Rectangle PaintBorderSelected()
        {
            int adding_x = 0, adding_y = 0;
            if (Height > Width) adding_x = (Height - Width) / 2;
            else if (Width > Height) adding_y = (Width - Height) / 2;
            return new System.Drawing.Rectangle(StartX - adding_x - 10, StartY - adding_y - 10, Math.Max(Width, Height) + 20, Math.Max(Width, Height) + 20);
        }

        public void Move(int delta_x, int delta_y)
        {
            StartX += delta_x;
            StartY += delta_y;

        }


        public void ChangeStartPoint(int x, int y)
        {
            if (Width + x < 5 || Height + y < 5) return;
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
