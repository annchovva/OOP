using System.Drawing;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Figures
{
    public class EllipseFigure : FigureBase
    {
        public override string FigureType => "Ellipse";

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override void Draw(Graphics g)
        {
            using var brush = new SolidBrush(Style.FillColor);
            using var pen = new Pen(Style.StrokeColor, Style.StrokeWidth);

            g.FillEllipse(brush, X, Y, Width, Height);
            g.DrawEllipse(pen, X, Y, Width, Height);
        }

        public override bool HitTest(PointF point)
        {
            return new RectangleF(X, Y, Width, Height).Contains(point);
        }

        public override void Move(float dx, float dy)
        {
            X += (int)dx;
            Y += (int)dy;
        }

        public override void Resize(RectangleF bounds)
        {
            X = (int)bounds.X;
            Y = (int)bounds.Y;
            Width = (int)bounds.Width;
            Height = (int)bounds.Height;
        }

        public override IFigure Clone()
        {
            return new EllipseFigure
            {
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                Rotation = Rotation,
                IsSelected = IsSelected,
                Style = new DrawingStyle
                {
                    FillColor = Style.FillColor,
                    StrokeColor = Style.StrokeColor,
                    StrokeWidth = Style.StrokeWidth
                }
            };
        }

        public override FigureDto ToDto() => new FigureDto { Type = FigureType, Id = Id };

        public override void FromDto(FigureDto dto) { }
    }
}
