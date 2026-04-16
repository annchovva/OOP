using MyPaint.Core;
using System.Drawing;

namespace MYPaint.Plugins
{
    public class Line : IFigure
    {
        public string Name { get; } = "Line";
        public Color Color { get; set; }
        public Color BorderColor { get; set; }
        public int Thickness { get; set; }
        public float Angle { get; set; }


        public int StartX { get; set; }
        public int StartY { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public int EndX;
        public int EndY;
        public void OnMouseDown(int x, int y)
        {
            StartX = x;
            StartY = y;
        }
        public void OnMouseUp(int x, int y, Graphics graphics)
        {
            OnMouseMove(x, y, graphics);
        }
        public void OnMouseMove(int x, int y, Graphics graphics)
        {
            EndX = x;
            EndY = y;
            using (Pen pen = new Pen(this.BorderColor, this.Thickness))
            {
                graphics.DrawLine(pen, StartX, StartY, x, y);
            }
        }

        public void Draw(Graphics graphics)
        {
            var state = graphics.Save();

            float centerX = (StartX + EndX) / 2f;
            float centerY = (StartY + EndY) / 2f;

            graphics.TranslateTransform(centerX, centerY);
            graphics.RotateTransform(Angle);
            graphics.TranslateTransform(-centerX, -centerY);

            using (Pen pen = new Pen(this.BorderColor, this.Thickness))
            {
                graphics.DrawLine(pen, StartX, StartY, EndX, EndY);
            }

            graphics.Restore(state);
        }

        public bool IsSelected(int x, int y)
        {
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLine(StartX, StartY, EndX, EndY);

                using (var matrix = new System.Drawing.Drawing2D.Matrix())
                {
                    float centerX = (StartX + EndX) / 2f;
                    float centerY = (StartY + EndY) / 2f;
                    matrix.RotateAt(Angle, new PointF(centerX, centerY));
                    path.Transform(matrix);
                }

                float clickArea = Math.Max(this.Thickness, 10f);
                using (var pen = new Pen(Color.Black, clickArea))
                {
                    return path.IsOutlineVisible(x, y, pen);
                }
            }
        }

        public System.Drawing.Rectangle PaintBorderSelected()
        {
            var start_x = Math.Min(StartX, EndX);
            var start_y = Math.Min(StartY, EndY);

            return new System.Drawing.Rectangle(start_x - 10, start_y - 10, Math.Abs(EndX - StartX) + 20, Math.Abs(EndY - StartY) + 20);
        }

        public void Move(int delta_x, int delta_y)
        {
            StartX += delta_x;
            EndX += delta_x;
            StartY += delta_y;
            EndY += delta_y;
        }
        public void ChangeStartPoint(int x, int y)
        {

            int ogr = 10;
            if (StartX < EndX && StartY > EndY)
            {
                if (Math.Abs(EndX - StartX - x) < ogr) return;
                if (Math.Abs(EndY - StartY + y) < ogr) return;
                StartX += x;
                StartY -= y;
            }
            else if (StartX < EndX && StartY < EndY)
            {
                if (Math.Abs(EndX - StartX - x) < ogr) return;
                if (Math.Abs(EndY - StartY - y) < ogr) return;
                StartX += x;
                StartY += y;
            }
            else if (StartX > EndX && StartY > EndY)
            {
                if (Math.Abs(EndX - StartX - x) < ogr) return;
                if (Math.Abs(EndY - StartY - y) < ogr) return;
                EndX += x;
                EndY += y;
            }
            else if (StartX > EndX && StartY < EndY)
            {
                if (Math.Abs(EndX - StartX - x) < ogr) return;
                if (Math.Abs(EndY - StartY + y) < ogr) return;
                EndX += x;
                EndY -= y;
            }



        }
        public void ChangeEndPoint(int x, int y)
        {
            int ogr = 10;
            if (StartX < EndX && StartY > EndY)
            {
                if (Math.Abs(EndX - StartX + x) < ogr) return;
                if (Math.Abs(EndY - StartY - y) < ogr) return;
                EndX += x;
                EndY -= y;
            }
            else if (StartX < EndX && StartY < EndY)
            {
                if (Math.Abs(EndX - StartX + x) < ogr) return;
                if (Math.Abs(EndY - StartY + y) < ogr) return;
                EndX += x;
                EndY += y;
            }
            else if (StartX > EndX && StartY > EndY)
            {
                if (Math.Abs(EndX - StartX + x) < ogr) return;
                if (Math.Abs(EndY - StartY + y) < ogr) return;
                StartX += x;
                StartY += y;
            }
            else if (StartX > EndX && StartY < EndY)
            {
                if (Math.Abs(EndX - StartX + x) < ogr) return;
                if (Math.Abs(EndY - StartY - y) < ogr) return;
                StartX += x;
                StartY -= y;
            }

        }
        public IFigure Clone()
        {
            return (IFigure)this.MemberwiseClone();
        }
    }
}
